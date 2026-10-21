using System.Data;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Architecture;

public sealed class IdentityDatabaseContractTests : ControllerTestBase
{
    [Fact(DisplayName = "Banco — tabela usuarios contém o contrato de segurança do Identity")]
    public async Task UsuariosTable_ContainsIdentitySecurityColumnsAndIndexes()
    {
        var connection = Db.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
            await connection.OpenAsync();

        var columns = new Dictionary<string, (string Type, string Extra)>(StringComparer.OrdinalIgnoreCase);
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT COLUMN_NAME, COLUMN_TYPE, EXTRA
                FROM information_schema.COLUMNS
                WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'usuarios'
                """;
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                columns.Add(reader.GetString(0), (reader.GetString(1), reader.GetString(2)));
        }

        foreach (var column in new[]
                 {
                     "UserName", "NormalizedUserName", "NormalizedEmail", "EmailConfirmed",
                     "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed",
                     "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount"
                 })
            Assert.Contains(column, columns.Keys);

        // Revisão do DDL: chave INT UNSIGNED AUTO_INCREMENT; a coluna Senha continua como era.
        Assert.Equal("int unsigned", columns["IdUsuario"].Type, ignoreCase: true);
        Assert.Contains("auto_increment", columns["IdUsuario"].Extra, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("longtext", columns["Senha"].Type, ignoreCase: true);

        var indexes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = """
                SELECT DISTINCT INDEX_NAME
                FROM information_schema.STATISTICS
                WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'usuarios'
                """;
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                indexes.Add(reader.GetString(0));
        }

        Assert.Contains("PRIMARY", indexes);
        Assert.Contains("UQ_Usuarios_NormalizedUserName", indexes);
        Assert.Contains("UQ_Usuarios_NormalizedEmail", indexes);
    }
}
