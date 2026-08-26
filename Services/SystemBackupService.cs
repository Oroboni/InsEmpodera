using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Empodera.Services;

public sealed class SystemBackupService(ApplicationDbContext context)
{
    public const int CurrentFormatVersion = 1;
    public const long MaxBackupBytes = 100L * 1024 * 1024;
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly ApplicationDbContext _context = context;

    public async Task<byte[]> ExportAsync(CancellationToken token)
    {
        var mappings = GetMappings();
        var document = new SystemBackupDocument
        {
            FormatVersion = CurrentFormatVersion,
            SchemaFingerprint = ComputeFingerprint(mappings),
            ExportedAtUtc = DateTime.UtcNow
        };
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync(token);
        try
        {
            foreach (var mapping in mappings)
                document.Tables.Add(await ReadTableAsync(connection, mapping, token));
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }
        return JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
    }

    public async Task<BackupImportResult> ImportMissingAsync(Stream stream, CancellationToken token)
    {
        SystemBackupDocument document;
        try
        {
            document = await JsonSerializer.DeserializeAsync<SystemBackupDocument>(stream, JsonOptions, token)
                ?? throw new InvalidDataException("O arquivo de backup está vazio.");
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("O arquivo não é um backup InsEmpodera válido.", exception);
        }

        var mappings = GetMappings();
        Validate(document, mappings);
        var tables = document.Tables.ToDictionary(table => table.Table, StringComparer.OrdinalIgnoreCase);
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync(token);
        var sqlite = (_context.Database.ProviderName ?? string.Empty).Contains("Sqlite", StringComparison.OrdinalIgnoreCase);
        DbTransaction? transaction = null;
        var inserted = 0;
        try
        {
            if (sqlite) await ExecuteAsync(connection, null, "PRAGMA foreign_keys=OFF", token);
            transaction = await connection.BeginTransactionAsync(token);
            if (!sqlite) await ExecuteAsync(connection, transaction, "SET FOREIGN_KEY_CHECKS=0", token);

            foreach (var mapping in mappings)
            foreach (var row in tables[mapping.TableName].Rows)
                inserted += await InsertIfMissingAsync(connection, transaction, mapping, row, sqlite, token);

            await ValidateDatabaseRelationshipsAsync(connection, transaction, mappings, token);
            if (!sqlite) await ExecuteAsync(connection, transaction, "SET FOREIGN_KEY_CHECKS=1", token);
            await transaction.CommitAsync(token);
            _context.ChangeTracker.Clear();
            return new BackupImportResult(document.Tables.Count, inserted, document.ExportedAtUtc);
        }
        catch
        {
            if (transaction != null) await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
        finally
        {
            if (!sqlite && connection.State == ConnectionState.Open)
            {
                try { await ExecuteAsync(connection, null, "SET FOREIGN_KEY_CHECKS=1", CancellationToken.None); } catch { }
            }
            if (sqlite && connection.State == ConnectionState.Open)
                await ExecuteAsync(connection, null, "PRAGMA foreign_keys=ON", CancellationToken.None);
            if (transaction != null) await transaction.DisposeAsync();
            if (shouldClose) await connection.CloseAsync();
        }
    }

    private async Task<SystemBackupTable> ReadTableAsync(DbConnection connection, TableMapping mapping, CancellationToken token)
    {
        var table = new SystemBackupTable
        {
            Entity = mapping.EntityType.ClrType.FullName ?? mapping.EntityType.Name,
            Table = mapping.TableName,
            Columns = mapping.Columns.Select(column => column.ColumnName).ToList()
        };
        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {string.Join(", ", mapping.Columns.Select(column => Quote(column.ColumnName)))} FROM {Quote(mapping.TableName)}";
        await using var reader = await command.ExecuteReaderAsync(token);
        while (await reader.ReadAsync(token))
        {
            var row = new List<JsonElement>(mapping.Columns.Count);
            for (var index = 0; index < mapping.Columns.Count; index++)
            {
                var value = reader.IsDBNull(index) ? null : reader.GetValue(index);
                row.Add(value == null
                    ? JsonSerializer.SerializeToElement<object?>(null, JsonOptions)
                    : JsonSerializer.SerializeToElement(value, value.GetType(), JsonOptions));
            }
            table.Rows.Add(row);
        }
        return table;
    }

    private static async Task<int> InsertIfMissingAsync(DbConnection connection, DbTransaction transaction, TableMapping mapping, List<JsonElement> row, bool sqlite, CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        var verb = sqlite ? "INSERT OR IGNORE" : "INSERT IGNORE";
        command.CommandText = $"{verb} INTO {Quote(mapping.TableName)} ({string.Join(", ", mapping.Columns.Select(column => Quote(column.ColumnName)))}) VALUES ({string.Join(", ", mapping.Columns.Select((_, index) => $"@p{index}"))})";
        for (var index = 0; index < mapping.Columns.Count; index++)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@p{index}";
            try
            {
                parameter.Value = ConvertValue(row[index], mapping.Columns[index].Property.ClrType) ?? DBNull.Value;
            }
            catch (Exception exception) when (exception is JsonException or FormatException or InvalidOperationException or OverflowException)
            {
                throw new InvalidDataException(
                    $"Valor inválido em {mapping.TableName}.{mapping.Columns[index].ColumnName}.", exception);
            }
            command.Parameters.Add(parameter);
        }
        return await command.ExecuteNonQueryAsync(token);
    }

    private static object? ConvertValue(JsonElement element, Type declaredType)
    {
        if (element.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined) return null;
        var type = Nullable.GetUnderlyingType(declaredType) ?? declaredType;
        if (type == typeof(string)) return element.ValueKind == JsonValueKind.String ? element.GetString() : element.ToString();
        if (type == typeof(bool))
        {
            if (element.ValueKind is JsonValueKind.True or JsonValueKind.False) return element.GetBoolean();
            if (element.ValueKind == JsonValueKind.Number && element.TryGetInt64(out var numeric)) return numeric != 0;
            if (element.ValueKind == JsonValueKind.String)
            {
                var value = element.GetString();
                if (bool.TryParse(value, out var boolean)) return boolean;
                if (long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out numeric)) return numeric != 0;
            }
            throw new FormatException("Booleano incompatível.");
        }
        if (type == typeof(DateTime)) return DateTime.Parse(element.GetString()!, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
        if (type == typeof(DateOnly)) return DateOnly.Parse(element.GetString()!, CultureInfo.InvariantCulture);
        if (type == typeof(TimeOnly)) return TimeOnly.Parse(element.GetString()!, CultureInfo.InvariantCulture);
        if (type == typeof(TimeSpan)) return TimeSpan.Parse(element.GetString()!, CultureInfo.InvariantCulture);
        if (type == typeof(Guid)) return element.GetGuid();
        if (type == typeof(byte[])) return element.GetBytesFromBase64();
        if (type.IsEnum) return JsonSerializer.Deserialize(element.GetRawText(), Enum.GetUnderlyingType(type), JsonOptions);
        return JsonSerializer.Deserialize(element.GetRawText(), type, JsonOptions);
    }

    private async Task ValidateDatabaseRelationshipsAsync(DbConnection connection, DbTransaction transaction, IReadOnlyList<TableMapping> mappings, CancellationToken token)
    {
        var byEntity = mappings.ToDictionary(mapping => mapping.EntityType);
        foreach (var dependent in mappings)
        foreach (var foreignKey in dependent.EntityType.GetForeignKeys())
        {
            if (!byEntity.TryGetValue(foreignKey.PrincipalEntityType, out var principal)) continue;
            var dependentStore = StoreObjectIdentifier.Table(dependent.TableName, dependent.EntityType.GetSchema());
            var principalStore = StoreObjectIdentifier.Table(principal.TableName, principal.EntityType.GetSchema());
            var pairs = foreignKey.Properties.Zip(foreignKey.PrincipalKey.Properties).ToArray();
            var join = string.Join(" AND ", pairs.Select(pair => $"d.{Quote(pair.First.GetColumnName(dependentStore)!)} = p.{Quote(pair.Second.GetColumnName(principalStore)!)}"));
            var hasValue = string.Join(" AND ", pairs.Select(pair => $"d.{Quote(pair.First.GetColumnName(dependentStore)!)} IS NOT NULL"));
            var principalMissing = $"p.{Quote(pairs[0].Second.GetColumnName(principalStore)!)} IS NULL";
            await using var command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = $"SELECT 1 FROM {Quote(dependent.TableName)} d LEFT JOIN {Quote(principal.TableName)} p ON {join} WHERE {hasValue} AND {principalMissing} LIMIT 1";
            if (await command.ExecuteScalarAsync(token) != null)
                throw new InvalidDataException($"O backup produziria vínculo inválido entre {dependent.TableName} e {principal.TableName}.");
        }
    }

    private void Validate(SystemBackupDocument document, IReadOnlyList<TableMapping> mappings)
    {
        if (document.Product != "InsEmpodera" || document.FormatVersion != CurrentFormatVersion)
            throw new InvalidDataException("Versão ou origem do backup incompatível.");
        if (document.SchemaFingerprint != ComputeFingerprint(mappings))
            throw new InvalidDataException("O backup pertence a uma estrutura de banco diferente desta versão.");
        if (document.Tables.Count != mappings.Count)
            throw new InvalidDataException("O backup não contém todas as tabelas do sistema.");
        var byName = document.Tables.ToDictionary(table => table.Table, StringComparer.OrdinalIgnoreCase);
        foreach (var mapping in mappings)
        {
            if (!byName.TryGetValue(mapping.TableName, out var table))
                throw new InvalidDataException($"Tabela ausente no backup: {mapping.TableName}.");
            var expected = mapping.Columns.Select(column => column.ColumnName).ToArray();
            if (!table.Columns.SequenceEqual(expected, StringComparer.OrdinalIgnoreCase) || table.Rows.Any(row => row.Count != expected.Length))
                throw new InvalidDataException($"Estrutura inválida na tabela {mapping.TableName}.");
        }
    }

    private List<TableMapping> GetMappings() => _context.Model.GetEntityTypes()
        .Where(entity => !entity.IsOwned() && entity.GetTableName() != null)
        .GroupBy(entity => entity.GetTableName()!, StringComparer.OrdinalIgnoreCase)
        .Select(group => group.First())
        .Select(entity =>
        {
            var table = entity.GetTableName()!;
            var store = StoreObjectIdentifier.Table(table, entity.GetSchema());
            var columns = entity.GetProperties().Select(property => new ColumnMapping(property, property.GetColumnName(store)!))
                .OrderByDescending(column => column.Property.IsPrimaryKey()).ThenBy(column => column.ColumnName, StringComparer.Ordinal).ToList();
            return new TableMapping(entity, table, columns);
        }).OrderBy(mapping => mapping.TableName, StringComparer.Ordinal).ToList();

    private static string ComputeFingerprint(IReadOnlyList<TableMapping> mappings)
    {
        var schema = string.Join("\n", mappings.SelectMany(mapping => mapping.Columns.Select(column => $"{mapping.TableName}|{column.ColumnName}|{column.Property.ClrType.FullName}|{column.Property.IsNullable}")));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(schema))).ToLowerInvariant();
    }

    private static async Task ExecuteAsync(DbConnection connection, DbTransaction? transaction, string sql, CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(token);
    }

    private static string Quote(string identifier) => $"`{identifier.Replace("`", "``", StringComparison.Ordinal)}`";
    private sealed record ColumnMapping(IProperty Property, string ColumnName);
    private sealed record TableMapping(IEntityType EntityType, string TableName, List<ColumnMapping> Columns);
}

public sealed record BackupImportResult(int TableCount, int InsertedRows, DateTime ExportedAtUtc);
