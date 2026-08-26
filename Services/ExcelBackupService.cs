using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using ClosedXML.Excel;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Empodera.Services;

public sealed class ExcelBackupService(ApplicationDbContext context)
{
    public const int CurrentFormatVersion = 2;
    public const long MaxBackupBytes = 100L * 1024 * 1024;
    private const int MaxCellTextLength = 30_000;
    private const int MaxImportRows = 2_000_000;
    private const string ManifestSheet = "_Backup";
    private const string NullSheet = "_Nulos";
    private const string LongTextSheet = "_TextosLongos";
    private readonly ApplicationDbContext _context = context;

    public async Task<byte[]> ExportAsync(CancellationToken token)
    {
        var mappings = GetMappings();
        using var workbook = CreateWorkbook(mappings);
        var nulls = workbook.Worksheets.Add(NullSheet);
        var longTexts = workbook.Worksheets.Add(LongTextSheet);
        WriteTechnicalHeaders(nulls, "Tabela", "Linha", "Coluna");
        WriteTechnicalHeaders(longTexts, "Tabela", "Linha", "Coluna", "Parte", "Conteúdo");
        var nullRow = 2;
        var longTextRow = 2;
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync(token);
        try
        {
            var manifest = workbook.Worksheet(ManifestSheet);
            var manifestRow = 9;
            foreach (var mapping in mappings)
            {
                var worksheet = workbook.Worksheets.Add(mapping.TableName);
                var rowCount = await WriteTableAsync(
                    connection, worksheet, mapping, nulls, longTexts,
                    () => nullRow++, () => longTextRow++, token);
                manifest.Cell(manifestRow, 1).Value = mapping.TableName;
                manifest.Cell(manifestRow, 2).Value = worksheet.Name;
                manifest.Cell(manifestRow, 3).Value = rowCount;
                manifestRow++;
            }
            manifest.Range(8, 1, manifestRow - 1, 3).CreateTable("TabelasDoBackup");
            manifest.Columns(1, 3).AdjustToContents();
        }
        finally
        {
            if (shouldClose) await connection.CloseAsync();
        }

        nulls.Visibility = XLWorksheetVisibility.VeryHidden;
        longTexts.Visibility = XLWorksheetVisibility.VeryHidden;
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<BackupImportResult> ImportMissingAsync(Stream stream, CancellationToken token)
    {
        using var workbook = OpenWorkbook(stream);
        var mappings = GetMappings();
        var manifest = ValidateAndReadManifest(workbook, mappings);
        var nullCells = ReadNullCells(workbook, manifest);
        var longTexts = ReadLongTexts(workbook, manifest);
        var connection = _context.Database.GetDbConnection();
        var shouldClose = connection.State != ConnectionState.Open;
        if (shouldClose) await connection.OpenAsync(token);
        var sqlite = (_context.Database.ProviderName ?? string.Empty)
            .Contains("Sqlite", StringComparison.OrdinalIgnoreCase);
        DbTransaction? transaction = null;
        var inserted = 0;
        try
        {
            if (sqlite) await ExecuteAsync(connection, null, "PRAGMA foreign_keys=OFF", token);
            transaction = await connection.BeginTransactionAsync(token);
            if (!sqlite) await ExecuteAsync(connection, transaction, "SET FOREIGN_KEY_CHECKS=0", token);

            foreach (var mapping in mappings)
            {
                var table = manifest.Tables[mapping.TableName];
                var worksheet = workbook.Worksheet(table.SheetName);
                for (var rowNumber = 1; rowNumber <= table.RowCount; rowNumber++)
                {
                    var values = new object?[mapping.Columns.Count];
                    for (var columnNumber = 1; columnNumber <= mapping.Columns.Count; columnNumber++)
                    {
                        var address = new BackupCell(mapping.TableName, rowNumber, columnNumber);
                        if (nullCells.Contains(address))
                            values[columnNumber - 1] = null;
                        else if (longTexts.TryGetValue(address, out var longText))
                            values[columnNumber - 1] = longText;
                        else
                            values[columnNumber - 1] = ConvertCell(
                                worksheet.Cell(rowNumber + 1, columnNumber),
                                mapping.Columns[columnNumber - 1].Property.ClrType,
                                mapping.TableName,
                                mapping.Columns[columnNumber - 1].ColumnName);
                    }
                    inserted += await InsertIfMissingAsync(
                        connection, transaction, mapping, values, sqlite, token);
                }
            }

            await ValidateDatabaseRelationshipsAsync(connection, transaction, mappings, token);
            if (!sqlite) await ExecuteAsync(connection, transaction, "SET FOREIGN_KEY_CHECKS=1", token);
            await transaction.CommitAsync(token);
            _context.ChangeTracker.Clear();
            return new BackupImportResult(mappings.Count, inserted, manifest.ExportedAtUtc);
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

    private static XLWorkbook CreateWorkbook(IReadOnlyList<TableMapping> mappings)
    {
        var workbook = new XLWorkbook();
        workbook.Properties.Title = "Backup geral InsEmpodera";
        workbook.Properties.Subject = "Backup completo para exportação e importação";
        var sheet = workbook.Worksheets.Add(ManifestSheet);
        sheet.Cell("A1").Value = "Backup geral InsEmpodera";
        sheet.Range("A1:C1").Merge();
        sheet.Cell("A1").Style.Font.Bold = true;
        sheet.Cell("A1").Style.Font.FontSize = 20;
        sheet.Cell("A1").Style.Font.FontColor = XLColor.White;
        sheet.Cell("A1").Style.Fill.BackgroundColor = XLColor.FromHtml("#722B7C");
        sheet.Row(1).Height = 34;
        sheet.Cell("A2").Value = "Arquivo completo para importar registros ausentes. Não altere os nomes das abas ou colunas.";
        sheet.Range("A2:C2").Merge();
        sheet.Cell("A3").Value = "Produto";
        sheet.Cell("B3").Value = "InsEmpodera";
        sheet.Cell("A4").Value = "Versão";
        sheet.Cell("B4").Value = CurrentFormatVersion;
        sheet.Cell("A5").Value = "Estrutura";
        sheet.Cell("B5").Value = ComputeFingerprint(mappings);
        sheet.Cell("A6").Value = "Exportado em UTC";
        sheet.Cell("B6").Value = DateTime.UtcNow.ToString("O", CultureInfo.InvariantCulture);
        sheet.Cell("A8").Value = "Tabela";
        sheet.Cell("B8").Value = "Aba";
        sheet.Cell("C8").Value = "Registros";
        sheet.Range("A3:A6").Style.Font.Bold = true;
        sheet.Range("A3:B6").Style.Fill.BackgroundColor = XLColor.FromHtml("#F6EEF7");
        sheet.SheetView.FreezeRows(8);
        return workbook;
    }

    private static void WriteTechnicalHeaders(IXLWorksheet sheet, params string[] headers)
    {
        for (var index = 0; index < headers.Length; index++)
            sheet.Cell(1, index + 1).Value = headers[index];
        sheet.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;
    }

    private static async Task<int> WriteTableAsync(
        DbConnection connection,
        IXLWorksheet worksheet,
        TableMapping mapping,
        IXLWorksheet nulls,
        IXLWorksheet longTexts,
        Func<int> nextNullRow,
        Func<int> nextLongTextRow,
        CancellationToken token)
    {
        for (var index = 0; index < mapping.Columns.Count; index++)
            worksheet.Cell(1, index + 1).Value = mapping.Columns[index].ColumnName;
        StyleTableHeader(worksheet, mapping.Columns.Count);

        await using var command = connection.CreateCommand();
        command.CommandText = $"SELECT {string.Join(", ", mapping.Columns.Select(column => Quote(column.ColumnName)))} FROM {Quote(mapping.TableName)}";
        await using var reader = await command.ExecuteReaderAsync(token);
        var dataRow = 0;
        while (await reader.ReadAsync(token))
        {
            dataRow++;
            for (var index = 0; index < mapping.Columns.Count; index++)
            {
                if (reader.IsDBNull(index))
                {
                    var technicalRow = nextNullRow();
                    nulls.Cell(technicalRow, 1).Value = mapping.TableName;
                    nulls.Cell(technicalRow, 2).Value = dataRow;
                    nulls.Cell(technicalRow, 3).Value = index + 1;
                    continue;
                }

                var value = NormalizeDatabaseValue(reader.GetValue(index), mapping.Columns[index].Property.ClrType);
                if (value is string text && text.Length > MaxCellTextLength)
                {
                    worksheet.Cell(dataRow + 1, index + 1).Value = "[Texto longo preservado no backup]";
                    var part = 0;
                    for (var offset = 0; offset < text.Length; offset += MaxCellTextLength)
                    {
                        var technicalRow = nextLongTextRow();
                        longTexts.Cell(technicalRow, 1).Value = mapping.TableName;
                        longTexts.Cell(technicalRow, 2).Value = dataRow;
                        longTexts.Cell(technicalRow, 3).Value = index + 1;
                        longTexts.Cell(technicalRow, 4).Value = ++part;
                        longTexts.Cell(technicalRow, 5).Value = text.Substring(offset, Math.Min(MaxCellTextLength, text.Length - offset));
                    }
                }
                else
                    SetCellValue(worksheet.Cell(dataRow + 1, index + 1), value);
            }
        }

        worksheet.SheetView.FreezeRows(1);
        worksheet.Range(1, 1, Math.Max(1, dataRow + 1), mapping.Columns.Count).SetAutoFilter();
        worksheet.Columns().AdjustToContents(8, 45);
        return dataRow;
    }

    private static object NormalizeDatabaseValue(object value, Type declaredType)
    {
        var type = Nullable.GetUnderlyingType(declaredType) ?? declaredType;
        if (type == typeof(bool))
            return value is bool boolean ? boolean : Convert.ToInt64(value, CultureInfo.InvariantCulture) != 0;
        if (type == typeof(DateTime))
            return (value is DateTime date ? date : DateTime.Parse(value.ToString()!, CultureInfo.InvariantCulture))
                .ToString("O", CultureInfo.InvariantCulture);
        if (type == typeof(TimeSpan))
            return (value is TimeSpan time ? time : TimeSpan.Parse(value.ToString()!, CultureInfo.InvariantCulture))
                .ToString("c", CultureInfo.InvariantCulture);
        if (type.IsEnum) return Convert.ToInt32(value, CultureInfo.InvariantCulture);
        return value;
    }

    private static void SetCellValue(IXLCell cell, object value)
    {
        cell.Value = value switch
        {
            bool boolean => boolean,
            int number => number,
            long number => number,
            short number => number,
            byte number => number,
            decimal number => number,
            double number => number,
            float number => number,
            _ => value.ToString() ?? string.Empty
        };
    }

    private static void StyleTableHeader(IXLWorksheet worksheet, int columnCount)
    {
        var header = worksheet.Range(1, 1, 1, columnCount);
        header.Style.Font.Bold = true;
        header.Style.Font.FontColor = XLColor.White;
        header.Style.Fill.BackgroundColor = XLColor.FromHtml("#722B7C");
    }

    private Manifest ValidateAndReadManifest(XLWorkbook workbook, IReadOnlyList<TableMapping> mappings)
    {
        if (!workbook.TryGetWorksheet(ManifestSheet, out var sheet))
            throw new InvalidDataException("O Excel não contém a aba de identificação do backup.");
        if (ReadText(sheet.Cell("B3")) != "InsEmpodera"
            || !int.TryParse(ReadText(sheet.Cell("B4")), NumberStyles.Integer, CultureInfo.InvariantCulture, out var version)
            || version != CurrentFormatVersion)
            throw new InvalidDataException("Versão ou origem do backup Excel incompatível.");
        if (ReadText(sheet.Cell("B5")) != ComputeFingerprint(mappings))
            throw new InvalidDataException("O backup Excel pertence a uma estrutura de banco diferente desta versão.");
        if (!DateTime.TryParse(ReadText(sheet.Cell("B6")), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var exportedAt))
            throw new InvalidDataException("A data de geração do backup Excel é inválida.");

        var tables = new Dictionary<string, ManifestTable>(StringComparer.OrdinalIgnoreCase);
        var row = 9;
        var totalRows = 0L;
        while (!sheet.Cell(row, 1).IsEmpty())
        {
            var tableName = ReadText(sheet.Cell(row, 1));
            var sheetName = ReadText(sheet.Cell(row, 2));
            if (!int.TryParse(ReadText(sheet.Cell(row, 3)), NumberStyles.Integer, CultureInfo.InvariantCulture, out var rowCount)
                || rowCount < 0 || rowCount > 1_048_575)
                throw new InvalidDataException($"Quantidade de registros inválida para {tableName}.");
            if (!tables.TryAdd(tableName, new ManifestTable(sheetName, rowCount, 0)))
                throw new InvalidDataException($"Tabela duplicada no backup Excel: {tableName}.");
            totalRows += rowCount;
            row++;
        }
        if (totalRows > MaxImportRows)
            throw new InvalidDataException("O backup Excel excede o limite de registros permitido.");
        if (tables.Count != mappings.Count)
            throw new InvalidDataException("O backup Excel não contém todas as tabelas do sistema.");

        foreach (var mapping in mappings)
        {
            if (!tables.TryGetValue(mapping.TableName, out var table)
                || !workbook.TryGetWorksheet(table.SheetName, out var dataSheet))
                throw new InvalidDataException($"Tabela ausente no backup Excel: {mapping.TableName}.");
            for (var column = 0; column < mapping.Columns.Count; column++)
                if (!ReadText(dataSheet.Cell(1, column + 1)).Equals(mapping.Columns[column].ColumnName, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Colunas inválidas na tabela {mapping.TableName}.");
            tables[mapping.TableName] = table with { ColumnCount = mapping.Columns.Count };
        }
        return new Manifest(exportedAt, tables);
    }

    private static HashSet<BackupCell> ReadNullCells(XLWorkbook workbook, Manifest manifest)
    {
        if (!workbook.TryGetWorksheet(NullSheet, out var sheet))
            throw new InvalidDataException("O backup Excel não contém o mapa de valores nulos.");
        var result = new HashSet<BackupCell>();
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var row = 2; row <= lastRow; row++)
        {
            var address = ReadTechnicalAddress(sheet, row, manifest);
            if (!result.Add(address))
                throw new InvalidDataException("O mapa de valores nulos contém uma referência duplicada.");
        }
        return result;
    }

    private static Dictionary<BackupCell, string> ReadLongTexts(XLWorkbook workbook, Manifest manifest)
    {
        if (!workbook.TryGetWorksheet(LongTextSheet, out var sheet))
            throw new InvalidDataException("O backup Excel não contém a área de textos longos.");
        var parts = new Dictionary<BackupCell, SortedDictionary<int, string>>();
        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 1;
        for (var row = 2; row <= lastRow; row++)
        {
            var address = ReadTechnicalAddress(sheet, row, manifest);
            if (!int.TryParse(ReadText(sheet.Cell(row, 4)), NumberStyles.Integer, CultureInfo.InvariantCulture, out var part)
                || part <= 0)
                throw new InvalidDataException("O backup contém uma parte inválida de texto longo.");
            if (!parts.TryGetValue(address, out var addressParts))
                parts[address] = addressParts = new SortedDictionary<int, string>();
            if (!addressParts.TryAdd(part, ReadText(sheet.Cell(row, 5))))
                throw new InvalidDataException("O backup contém uma parte duplicada de texto longo.");
        }
        return parts.ToDictionary(
            item => item.Key,
            item => string.Concat(item.Value.OrderBy(part => part.Key).Select(part => part.Value)));
    }

    private static BackupCell ReadTechnicalAddress(IXLWorksheet sheet, int row, Manifest manifest)
    {
        var table = ReadText(sheet.Cell(row, 1));
        if (!manifest.Tables.TryGetValue(table, out var tableInfo)
            || !int.TryParse(ReadText(sheet.Cell(row, 2)), NumberStyles.Integer, CultureInfo.InvariantCulture, out var dataRow)
            || !int.TryParse(ReadText(sheet.Cell(row, 3)), NumberStyles.Integer, CultureInfo.InvariantCulture, out var column)
            || dataRow <= 0 || dataRow > tableInfo.RowCount
            || column <= 0 || column > tableInfo.ColumnCount)
            throw new InvalidDataException("O backup contém uma referência técnica inválida.");
        return new BackupCell(table, dataRow, column);
    }

    private static XLWorkbook OpenWorkbook(Stream stream)
    {
        try { return new XLWorkbook(stream); }
        catch (Exception exception)
        {
            throw new InvalidDataException("O arquivo não é um backup Excel válido do InsEmpodera.", exception);
        }
    }

    private static object? ConvertCell(IXLCell cell, Type declaredType, string table, string column)
    {
        try
        {
            if (cell.HasFormula)
                throw new InvalidDataException($"Fórmulas não são permitidas em {table}.{column}.");
            var type = Nullable.GetUnderlyingType(declaredType) ?? declaredType;
            if (type == typeof(string)) return cell.GetString();
            if (type == typeof(bool)) return cell.GetValue<bool>();
            if (type == typeof(int)) return cell.GetValue<int>();
            if (type == typeof(long)) return cell.GetValue<long>();
            if (type == typeof(short)) return cell.GetValue<short>();
            if (type == typeof(byte)) return cell.GetValue<byte>();
            if (type == typeof(decimal)) return cell.GetValue<decimal>();
            if (type == typeof(double)) return cell.GetValue<double>();
            if (type == typeof(float)) return cell.GetValue<float>();
            if (type == typeof(DateTime)) return DateTime.Parse(cell.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            if (type == typeof(TimeSpan)) return TimeSpan.Parse(cell.GetString(), CultureInfo.InvariantCulture);
            if (type == typeof(Guid)) return Guid.Parse(cell.GetString());
            if (type == typeof(byte[])) return Convert.FromBase64String(cell.GetString());
            if (type.IsEnum) return Enum.ToObject(type, cell.GetValue<int>());
            return Convert.ChangeType(cell.GetString(), type, CultureInfo.InvariantCulture);
        }
        catch (InvalidDataException) { throw; }
        catch (Exception exception) when (exception is FormatException or InvalidCastException or OverflowException)
        {
            throw new InvalidDataException($"Valor inválido em {table}.{column}.", exception);
        }
    }

    private static string ReadText(IXLCell cell)
    {
        if (cell.HasFormula) throw new InvalidDataException("O backup Excel não pode conter fórmulas.");
        return cell.GetString();
    }

    private static async Task<int> InsertIfMissingAsync(
        DbConnection connection,
        DbTransaction transaction,
        TableMapping mapping,
        IReadOnlyList<object?> row,
        bool sqlite,
        CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        var verb = sqlite ? "INSERT OR IGNORE" : "INSERT IGNORE";
        command.CommandText = $"{verb} INTO {Quote(mapping.TableName)} ({string.Join(", ", mapping.Columns.Select(column => Quote(column.ColumnName)))}) VALUES ({string.Join(", ", mapping.Columns.Select((_, index) => $"@p{index}"))})";
        for (var index = 0; index < mapping.Columns.Count; index++)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = $"@p{index}";
            parameter.Value = row[index] ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
        return await command.ExecuteNonQueryAsync(token);
    }

    private async Task ValidateDatabaseRelationshipsAsync(
        DbConnection connection,
        DbTransaction transaction,
        IReadOnlyList<TableMapping> mappings,
        CancellationToken token)
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

    private List<TableMapping> GetMappings() => _context.Model.GetEntityTypes()
        .Where(entity => !entity.IsOwned() && entity.GetTableName() != null)
        .GroupBy(entity => entity.GetTableName()!, StringComparer.OrdinalIgnoreCase)
        .Select(group => group.First())
        .Select(entity =>
        {
            var table = entity.GetTableName()!;
            var store = StoreObjectIdentifier.Table(table, entity.GetSchema());
            var columns = entity.GetProperties()
                .Select(property => new ColumnMapping(property, property.GetColumnName(store)!))
                .OrderByDescending(column => column.Property.IsPrimaryKey())
                .ThenBy(column => column.ColumnName, StringComparer.Ordinal)
                .ToList();
            return new TableMapping(entity, table, columns);
        })
        .OrderBy(mapping => mapping.TableName, StringComparer.Ordinal)
        .ToList();

    private static string ComputeFingerprint(IReadOnlyList<TableMapping> mappings)
    {
        var schema = string.Join("\n", mappings.SelectMany(mapping => mapping.Columns.Select(column =>
            $"{mapping.TableName}|{column.ColumnName}|{column.Property.ClrType.FullName}|{column.Property.IsNullable}")));
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(schema))).ToLowerInvariant();
    }

    private static async Task ExecuteAsync(
        DbConnection connection,
        DbTransaction? transaction,
        string sql,
        CancellationToken token)
    {
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        await command.ExecuteNonQueryAsync(token);
    }

    private static string Quote(string identifier) => $"`{identifier.Replace("`", "``", StringComparison.Ordinal)}`";
    private sealed record ColumnMapping(IProperty Property, string ColumnName);
    private sealed record TableMapping(IEntityType EntityType, string TableName, List<ColumnMapping> Columns);
    private sealed record Manifest(DateTime ExportedAtUtc, Dictionary<string, ManifestTable> Tables);
    private sealed record ManifestTable(string SheetName, int RowCount, int ColumnCount);
    private sealed record BackupCell(string Table, int Row, int Column);
}
