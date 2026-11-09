using System.Data.Common;
using InsEmpodera.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace InsEmpodera.Tests.Architecture;

/// <summary>
/// Checklist da revisão técnica do DDL (docs/revisao-ddl-mysql.md, seção 9) verificado no banco
/// criado pelo script oficial Database/Empodera.schema.sql.
/// </summary>
public sealed class RevisaoDdlTests : ControllerTestBase
{
    [Fact(DisplayName = "DDL — scripts de criação e de migração estão atualizados com o modelo do EF")]
    public void Scripts_EstaoAtualizados()
    {
        var raiz = new DirectoryInfo(AppContext.BaseDirectory);
        while (!File.Exists(Path.Combine(raiz!.FullName, "InsEmpodera.csproj")))
            raiz = raiz.Parent;
        string Ler(string nome) => File.ReadAllText(Path.Combine(raiz.FullName, "Database", nome)).Replace("\r\n", "\n");

        Assert.True(Ler("Empodera.schema.sql") == GerarSchema.GeradorEsquema.Gerar(),
            "Database/Empodera.schema.sql está desatualizado. Rode: dotnet run --project scripts/GerarSchema");
        Assert.True(Ler("migracao-uuid-para-int.sql") == GerarSchema.GeradorMigracao.Gerar(),
            "Database/migracao-uuid-para-int.sql está desatualizado. Rode: dotnet run --project scripts/GerarSchema");
    }

    [Fact(DisplayName = "DDL — 34 tabelas com PK INT UNSIGNED AUTO_INCREMENT")]
    public async Task ChavesPrimarias_SaoIntUnsignedAutoIncrement()
    {
        var pks = await ConsultarAsync("""
            SELECT c.TABLE_NAME, c.COLUMN_TYPE, c.EXTRA
            FROM information_schema.KEY_COLUMN_USAGE k
            JOIN information_schema.COLUMNS c
              ON c.TABLE_SCHEMA = k.TABLE_SCHEMA AND c.TABLE_NAME = k.TABLE_NAME AND c.COLUMN_NAME = k.COLUMN_NAME
            WHERE k.TABLE_SCHEMA = DATABASE() AND k.CONSTRAINT_NAME = 'PRIMARY'
            """);

        Assert.Equal(34, pks.Count);
        Assert.All(pks, pk =>
        {
            Assert.Equal("int unsigned", pk[1]);
            Assert.Contains("auto_increment", pk[2]);
        });
    }

    [Fact(DisplayName = "DDL — toda coluna que referencia outra tabela tem FK nomeada e do mesmo tipo da PK")]
    public async Task Referencias_TemChaveEstrangeiraDoMesmoTipo()
    {
        // Colunas IdXxx (Id + letra maiúscula; não pega Idade nem IdiomaPreferido) que não são a PK da própria tabela.
        var referencias = await ConsultarAsync("""
            SELECT c.TABLE_NAME, c.COLUMN_NAME
            FROM information_schema.COLUMNS c
            WHERE c.TABLE_SCHEMA = DATABASE() AND REGEXP_LIKE(c.COLUMN_NAME, '^Id[A-Z]', 'c') AND c.COLUMN_KEY <> 'PRI'
            """);
        var fks = await ConsultarAsync("""
            SELECT k.TABLE_NAME, k.COLUMN_NAME, k.CONSTRAINT_NAME, filha.COLUMN_TYPE, pai.COLUMN_TYPE
            FROM information_schema.KEY_COLUMN_USAGE k
            JOIN information_schema.COLUMNS filha
              ON filha.TABLE_SCHEMA = k.TABLE_SCHEMA AND filha.TABLE_NAME = k.TABLE_NAME AND filha.COLUMN_NAME = k.COLUMN_NAME
            JOIN information_schema.COLUMNS pai
              ON pai.TABLE_SCHEMA = k.REFERENCED_TABLE_SCHEMA AND pai.TABLE_NAME = k.REFERENCED_TABLE_NAME AND pai.COLUMN_NAME = k.REFERENCED_COLUMN_NAME
            WHERE k.TABLE_SCHEMA = DATABASE() AND k.REFERENCED_TABLE_NAME IS NOT NULL
            """);

        var comFk = fks.Select(f => (f[0], f[1])).ToHashSet();
        Assert.All(referencias, r => Assert.Contains((r[0], r[1]), comFk));
        Assert.All(fks, f =>
        {
            Assert.StartsWith("FK_", f[2]);
            Assert.Equal(f[4], f[3]);
        });
    }

    [Fact(DisplayName = "DDL — restrições nomeadas no padrão FK_/UQ_/CK_")]
    public async Task Restricoes_SeguemOPadraoDeNomes()
    {
        var restricoes = await ConsultarAsync("""
            SELECT CONSTRAINT_NAME, CONSTRAINT_TYPE
            FROM information_schema.TABLE_CONSTRAINTS
            WHERE TABLE_SCHEMA = DATABASE() AND CONSTRAINT_TYPE <> 'PRIMARY KEY'
            """);

        Assert.All(restricoes, r =>
        {
            var prefixo = r[1] switch { "FOREIGN KEY" => "FK_", "UNIQUE" => "UQ_", "CHECK" => "CK_", _ => "?" };
            Assert.StartsWith(prefixo, r[0]);
            Assert.DoesNotContain("ibfk", r[0]);
        });
    }

    [Fact(DisplayName = "DDL — collation herdado do banco, sem utf8mb3, sem TEXT/LONGTEXT, sem INT(n) e DATETIME sem precisão")]
    public async Task Tipos_SeguemARevisao()
    {
        var colunas = await ConsultarAsync("""
            SELECT TABLE_NAME, COLUMN_NAME, COLUMN_TYPE, IFNULL(CHARACTER_SET_NAME, ''), IFNULL(COLLATION_NAME, ''),
                   IFNULL(DATETIME_PRECISION, 0), DATA_TYPE
            FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE()
            """);

        Assert.All(colunas, c =>
        {
            // O hash da senha é a única coluna com conjunto próprio (ASCII binário, revisão 5.4).
            var senha = c[0] == "usuarios" && c[1] == "Senha";
            if (c[3] != "") Assert.Equal(senha ? "ascii" : "utf8mb4", c[3]);
            if (c[4] != "") Assert.Equal(senha ? "ascii_bin" : "utf8mb4_0900_ai_ci", c[4]);
            Assert.DoesNotContain(c[6], new[] { "tinytext", "text", "mediumtext", "longtext" });
            Assert.DoesNotMatch(@"^(tiny|small|medium|big)?int\(\d+\)", c[2]);
            if (c[6] is "datetime" or "time") Assert.Equal("0", c[5]);
        });
        var tabelas = await ConsultarAsync("""
            SELECT TABLE_NAME, TABLE_COLLATION FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE()
            """);
        Assert.All(tabelas, t => Assert.Equal("utf8mb4_0900_ai_ci", t[1]));
    }

    [Fact(DisplayName = "DDL — senha em VARCHAR(255) ASCII binário (hash do Identity)")]
    public async Task Senha_EhVarcharAsciiBinario()
    {
        var senha = await ConsultarAsync("""
            SELECT COLUMN_TYPE, CHARACTER_SET_NAME, COLLATION_NAME, IS_NULLABLE FROM information_schema.COLUMNS
            WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'usuarios' AND COLUMN_NAME = 'Senha'
            """);

        Assert.Equal(new[] { "varchar(255)", "ascii", "ascii_bin", "NO" }, Assert.Single(senha));
    }

    private async Task<List<string[]>> ConsultarAsync(string sql)
    {
        var conexao = Db.Database.GetDbConnection();
        if (conexao.State != System.Data.ConnectionState.Open)
            await conexao.OpenAsync();
        await using DbCommand comando = conexao.CreateCommand();
        comando.CommandText = sql;
        await using var leitor = await comando.ExecuteReaderAsync();
        var linhas = new List<string[]>();
        while (await leitor.ReadAsync())
            linhas.Add(Enumerable.Range(0, leitor.FieldCount).Select(i => Convert.ToString(leitor.GetValue(i))!).ToArray());
        return linhas;
    }
}
