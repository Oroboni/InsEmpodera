using System.Globalization;
using System.Text;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GerarSchema;

/// <summary>
/// Gera Database/Empodera.schema.sql a partir do modelo do EF (Data/ApplicationDbContext*.cs),
/// no formato pedido na revisão técnica do DDL:
///   1. CREATE TABLE só com as colunas (sem charset/collation: herdados do banco);
///   2. ALTER TABLE com PRIMARY KEY + AUTO_INCREMENT, UNIQUE e CHECK nomeados;
///   3. ALTER TABLE com as chaves estrangeiras nomeadas e a regra ON DELETE explícita;
///   4. dados iniciais (perfis, usuários e permissões de exemplo).
/// Sem SET FOREIGN_KEY_CHECKS = 0: a ordem dos INSERT respeita as chaves estrangeiras.
/// </summary>
public static class GeradorEsquema
{
    public static string Gerar()
    {
        // O texto do script não depende do servidor; só precisa de um contexto MySQL para ler o modelo.
        var opcoes = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql("Server=localhost;Database=gerar_schema", new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        using var db = new ApplicationDbContext(opcoes);
        var modelo = db.GetService<IDesignTimeModel>().Model;
        var entidades = modelo.GetEntityTypes().OrderBy(e => e.GetTableName(), StringComparer.Ordinal).ToList();

        var sql = new StringBuilder();
        sql.Append("""
            -- =====================================================================
            -- EcoRedes / Empodera — esquema do banco (MySQL 8.4 LTS ou posterior)
            -- Gerado por scripts/GerarSchema a partir de Data/ApplicationDbContext*.cs.
            -- Não edite à mão: altere o modelo e rode
            --   dotnet run --project scripts/GerarSchema
            -- Decisões e checklist da revisão: docs/revisao-ddl-mysql.md
            --
            -- Execute num banco vazio. O script não apaga bancos nem desliga a
            -- verificação de chaves estrangeiras.
            -- =====================================================================

            -- Padrão do banco: as tabelas herdam o conjunto de caracteres e o collation.
            ALTER DATABASE CHARACTER SET utf8mb4 COLLATE utf8mb4_0900_ai_ci;


            -- ---------------------------------------------------------------------
            -- 1. Tabelas (somente colunas)
            -- ---------------------------------------------------------------------

            """);

        foreach (var entidade in entidades)
            EscreverTabela(sql, entidade);

        sql.Append("""

            -- ---------------------------------------------------------------------
            -- 2. Chaves primárias (INT UNSIGNED AUTO_INCREMENT), unicidade e verificações
            -- ---------------------------------------------------------------------

            """);
        foreach (var entidade in entidades)
            EscreverRestricoes(sql, entidade);

        sql.Append("""

            -- ---------------------------------------------------------------------
            -- 3. Chaves estrangeiras (nome FK_<Filha>_<Pai>, regra de exclusão explícita)
            -- ---------------------------------------------------------------------

            """);
        foreach (var entidade in entidades)
            EscreverChavesEstrangeiras(sql, entidade);

        sql.Append("""

            -- ---------------------------------------------------------------------
            -- 4. Dados iniciais fictícios: perfis, usuários de exemplo e permissões
            --    (os perfis padrão não têm usuário criador: IdUsuario fica NULL)
            -- ---------------------------------------------------------------------

            """);
        foreach (var tabela in new[] { "perfis", "usuarios", "permissoes" })
            EscreverDados(sql, entidades.Single(e => e.GetTableName() == tabela));

        return sql.ToString().Replace("\r\n", "\n");
    }

    private static StoreObjectIdentifier Tabela(IEntityType e) => StoreObjectIdentifier.Table(e.GetTableName()!, null);

    // Colunas na ordem em que aparecem na classe; colunas só do banco (auditoria) no fim.
    private static List<IProperty> Colunas(IEntityType e)
    {
        var ordem = e.ClrType.GetProperties().Select((p, i) => (p.Name, i)).ToDictionary(x => x.Name, x => x.i);
        return e.GetProperties()
            .OrderBy(p => p.IsPrimaryKey() ? -1 : ordem.GetValueOrDefault(p.Name, int.MaxValue))
            .ThenBy(p => p.GetColumnName(Tabela(e)) == "DtModificacao" ? 1 : 0)
            .ToList();
    }

    private static string Tipo(IProperty p, StoreObjectIdentifier t)
    {
        var tipo = p.GetColumnType(t).ToUpperInvariant();
        return tipo switch
        {
            "INT" when p.ClrType == typeof(int) || p.ClrType == typeof(int?) => "INT",
            _ => tipo
        };
    }

    private static void EscreverTabela(StringBuilder sql, IEntityType e)
    {
        var t = Tabela(e);
        var colunas = Colunas(e);
        var largura = colunas.Max(c => c.GetColumnName(t)!.Length) + 1;
        sql.Append($"CREATE TABLE {e.GetTableName()} (\n");
        for (var i = 0; i < colunas.Count; i++)
        {
            var c = colunas[i];
            var nome = c.GetColumnName(t)!;
            var linha = new StringBuilder($"    {nome.PadRight(largura)}{Tipo(c, t)} {(c.IsColumnNullable(t) ? "NULL" : "NOT NULL")}");
            if (c.GetDefaultValueSql(t) is { } padraoSql)
                linha.Append($" DEFAULT {padraoSql}");
            else if (c.FindAnnotation(RelationalAnnotationNames.DefaultValue)?.Value is { } padrao)
                linha.Append($" DEFAULT {Literal(c, padrao)}");
            if (nome == "DtModificacao")
                linha.Append(" ON UPDATE CURRENT_TIMESTAMP");
            sql.Append(linha).Append(i < colunas.Count - 1 ? ",\n" : "\n");
        }
        sql.Append(");\n\n");
    }

    private static void EscreverRestricoes(StringBuilder sql, IEntityType e)
    {
        var t = Tabela(e);
        var pk = e.FindPrimaryKey()!.Properties.Single();
        var colunaPk = pk.GetColumnName(t)!;
        var partes = new List<string>
        {
            $"ADD PRIMARY KEY ({colunaPk})",
            $"MODIFY {colunaPk} {Tipo(pk, t)} NOT NULL AUTO_INCREMENT"
        };
        foreach (var indice in e.GetIndexes().Where(i => i.IsUnique).OrderBy(i => i.GetDatabaseName(t), StringComparer.Ordinal))
            partes.Add($"ADD CONSTRAINT {indice.GetDatabaseName(t)} UNIQUE ({string.Join(", ", indice.Properties.Select(p => p.GetColumnName(t)))})");
        foreach (var check in e.GetCheckConstraints().OrderBy(c => c.Name, StringComparer.Ordinal))
            partes.Add($"ADD CONSTRAINT {check.Name} CHECK ({check.Sql})");
        sql.Append($"ALTER TABLE {e.GetTableName()}\n    {string.Join(",\n    ", partes)};\n\n");
    }

    private static void EscreverChavesEstrangeiras(StringBuilder sql, IEntityType e)
    {
        var t = Tabela(e);
        var fks = e.GetForeignKeys().OrderBy(f => f.GetConstraintName(), StringComparer.Ordinal).ToList();
        if (fks.Count == 0) return;
        var partes = fks.Select(fk =>
        {
            var pai = fk.PrincipalEntityType;
            var colunaPai = fk.PrincipalKey.Properties.Single().GetColumnName(Tabela(pai));
            var regra = fk.DeleteBehavior switch
            {
                DeleteBehavior.Cascade => "CASCADE",
                DeleteBehavior.SetNull => "SET NULL",
                _ => "RESTRICT"
            };
            return $"ADD CONSTRAINT {fk.GetConstraintName()}\n        FOREIGN KEY ({fk.Properties.Single().GetColumnName(t)}) REFERENCES {pai.GetTableName()} ({colunaPai})\n        ON DELETE {regra} ON UPDATE RESTRICT";
        });
        sql.Append($"ALTER TABLE {e.GetTableName()}\n    {string.Join(",\n    ", partes)};\n\n");
    }

    private static void EscreverDados(StringBuilder sql, IEntityType e)
    {
        var t = Tabela(e);
        var linhas = e.GetSeedData().ToList();
        if (linhas.Count == 0) return;
        var colunas = Colunas(e).Where(c => linhas.Any(l => l.ContainsKey(c.Name) && l[c.Name] is not null)).ToList();
        sql.Append($"INSERT INTO {e.GetTableName()} ({string.Join(", ", colunas.Select(c => c.GetColumnName(t)))}) VALUES\n");
        sql.Append(string.Join(",\n", linhas.Select(l =>
            "    (" + string.Join(", ", colunas.Select(c => Literal(c, l.TryGetValue(c.Name, out var v) ? v : null))) + ")")));
        sql.Append(";\n\n");
    }

    private static string Literal(IProperty p, object? valor)
    {
        if (valor is not null && p.GetTypeMapping().Converter is { } conversor)
            valor = conversor.ConvertToProvider(valor);
        return valor switch
        {
            null => "NULL",
            string s => "'" + s.Replace("\\", "\\\\").Replace("'", "''") + "'",
            DateTime d => d.TimeOfDay == TimeSpan.Zero && p.GetColumnType() == "date"
                ? $"'{d:yyyy-MM-dd}'"
                : $"'{d:yyyy-MM-dd HH:mm:ss}'",
            bool b => b ? "1" : "0",
            Enum en => Convert.ToInt32(en, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => valor.ToString()!
        };
    }
}
