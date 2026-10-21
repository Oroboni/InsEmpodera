using System.Text;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;

namespace GerarSchema;

/// <summary>
/// Gera Database/migracao-uuid-para-int.sql: copia os dados do banco atual (chaves UUID em BINARY(16),
/// nomes antigos de colunas) para o esquema revisado (chaves INT UNSIGNED, nomes novos).
/// No banco antigo, cada coluna tem o nome da propriedade C# (o modelo não renomeava colunas).
/// </summary>
public static class GeradorMigracao
{
    private const string Antigo = "empodera_antigo";
    private const string Novo = "empodera";

    public static string Gerar()
    {
        var opcoes = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseMySql("Server=localhost;Database=gerar_schema", new MySqlServerVersion(new Version(8, 4, 0)))
            .Options;
        using var db = new ApplicationDbContext(opcoes);
        var modelo = db.GetService<IDesignTimeModel>().Model;
        var entidades = modelo.GetEntityTypes().ToList();
        var ordem = OrdemDeInsercao(entidades);

        var sql = new StringBuilder();
        sql.Append($"""
            -- =====================================================================
            -- Migração dos dados: chaves UUID (BINARY(16)) -> INT UNSIGNED AUTO_INCREMENT
            -- Gerado por scripts/GerarSchema. Não edite à mão.
            --
            -- Antes de rodar (MySQL 8.4 LTS ou posterior):
            --   1. Faça backup do banco atual.
            --   2. Importe o banco atual num schema chamado `{Antigo}`.
            --   3. Crie o banco novo `{Novo}` com Database/Empodera.schema.sql e NÃO
            --      inicie a aplicação antes desta migração.
            --   4. Rode:  mysql -u <usuario> -p < Database/migracao-uuid-para-int.sql
            --
            -- O script para sem gravar nada se encontrar problemas nos dados antigos
            -- (registros órfãos, textos maiores que o novo tamanho, valores fora do
            -- domínio, campos obrigatórios vazios ou duplicidades). A lista aparece
            -- na tela; corrija no banco antigo e rode de novo.
            --
            -- Os novos Ids seguem a ordem de criação (DtCriacao) de cada tabela.
            -- Detalhes: docs/revisao-ddl-mysql.md, seção "Migração dos dados".
            -- =====================================================================

            USE {Novo};

            -- ---------------------------------------------------------------------
            -- 1. Verificações (nada é gravado se houver problema)
            -- ---------------------------------------------------------------------

            DROP TEMPORARY TABLE IF EXISTS migracao_problemas;
            CREATE TEMPORARY TABLE migracao_problemas (
                problema   VARCHAR(255) NOT NULL,
                quantidade INT UNSIGNED NOT NULL
            );

            -- O banco novo só pode ter os dados iniciais do script de criação.

            """);
        foreach (var e in entidades.Where(e => e.GetTableName() is not ("perfis" or "usuarios" or "permissoes")).OrderBy(e => e.GetTableName()))
            Problema(sql, $"banco novo já tem dados em {e.GetTableName()}", $"SELECT COUNT(*) FROM {Novo}.{e.GetTableName()}");

        foreach (var e in entidades.OrderBy(e => e.GetTableName()))
            EscreverVerificacoes(sql, e);

        sql.Append($"""

            SELECT problema AS `Problema encontrado`, quantidade AS `Registros` FROM migracao_problemas;

            DROP PROCEDURE IF EXISTS migracao_parar_se_houver_problemas;
            DELIMITER //
            CREATE PROCEDURE migracao_parar_se_houver_problemas()
            BEGIN
                IF (SELECT COUNT(*) FROM migracao_problemas) > 0 THEN
                    SIGNAL SQLSTATE '45000'
                        SET MESSAGE_TEXT = 'Migração interrompida: corrija os problemas listados acima no banco antigo.';
                END IF;
            END //
            DELIMITER ;
            CALL migracao_parar_se_houver_problemas();
            DROP PROCEDURE migracao_parar_se_houver_problemas;


            -- ---------------------------------------------------------------------
            -- 2. Mapas de Ids (UUID antigo -> novo Id inteiro, na ordem de criação)
            -- ---------------------------------------------------------------------

            """);
        foreach (var e in ordem)
            EscreverMapa(sql, e);

        sql.Append($"""

            -- ---------------------------------------------------------------------
            -- 3. Cópia dos dados (numa transação: ou copia tudo, ou nada)
            -- ---------------------------------------------------------------------

            -- "@[Nome](ator:<uuid>)" -> "@[Nome](ator:<novo id>)" no Diário de Processo Pessoal.
            -- DtModificacao = DtModificacao mantém a data de modificação original.
            -- (CREATE/DROP PROCEDURE encerram transações no MySQL, por isso ficam fora dela.)
            DROP PROCEDURE IF EXISTS migracao_mencoes;
            DELIMITER //
            CREATE PROCEDURE migracao_mencoes()
            BEGIN
                REPEAT
                    UPDATE {Novo}.diariosprocessopessoal d
                    JOIN {Antigo}.migracao_mapa_atores m
                      ON LOCATE(CONCAT('(ator:', BIN_TO_UUID(m.antigo), ')'), d.Descricao) > 0
                    SET d.Descricao = REPLACE(d.Descricao, CONCAT('(ator:', BIN_TO_UUID(m.antigo), ')'), CONCAT('(ator:', m.novo, ')')),
                        d.DtModificacao = d.DtModificacao;
                UNTIL ROW_COUNT() = 0 END REPEAT;
            END //
            DELIMITER ;

            START TRANSACTION;

            -- Dados iniciais do script de criação dão lugar aos dados reais.
            DELETE FROM {Novo}.permissoes;
            DELETE FROM {Novo}.usuarios;
            DELETE FROM {Novo}.perfis;


            """);
        var inseridas = new HashSet<string>();
        var adiadas = new List<(IEntityType Entidade, IForeignKey Fk)>();
        foreach (var e in ordem)
        {
            EscreverCopia(sql, e, inseridas, adiadas);
            inseridas.Add(e.GetTableName()!);
        }
        foreach (var (e, fk) in adiadas)
            EscreverAtualizacaoAdiada(sql, e, fk);

        sql.Append($"""
            -- Menções no Diário de Processo Pessoal (procedimento criado antes da transação).
            CALL migracao_mencoes();

            COMMIT;

            DROP PROCEDURE migracao_mencoes;


            -- ---------------------------------------------------------------------
            -- 4. Fotos e cores dos usuários (App_Data)
            -- Os arquivos usavam o UUID sem hífens no nome; agora usam o novo Id.
            -- Renomeie conforme esta lista (docs/revisao-ddl-mysql.md traz o comando):
            -- ---------------------------------------------------------------------

            SELECT REPLACE(BIN_TO_UUID(antigo), '-', '') AS `Nome antigo`, novo AS `Nome novo`
            FROM {Antigo}.migracao_mapa_usuarios
            ORDER BY novo;

            """);
        return sql.ToString().Replace("\r\n", "\n");
    }

    // ── Verificações ──────────────────────────────────────────────────────

    private static void EscreverVerificacoes(StringBuilder sql, IEntityType e)
    {
        var tabela = e.GetTableName()!;
        var t = StoreObjectIdentifier.Table(tabela, null);
        sql.Append($"-- {tabela}\n");

        foreach (var fk in e.GetForeignKeys())
        {
            var coluna = ColunaAntiga(e, fk.Properties[0]);
            var pai = fk.PrincipalEntityType;
            var pkPai = pai.FindPrimaryKey()!.Properties[0].Name;
            // Referência opcional quebrada (inclusive o UUID zerado) vira NULL, sem bloquear.
            if (fk.Properties[0].IsNullable)
                continue;
            Problema(sql, $"{tabela}.{coluna} aponta para {pai.GetTableName()} inexistente",
                $"SELECT COUNT(*) FROM {Antigo}.{tabela} c LEFT JOIN {Antigo}.{pai.GetTableName()} p ON p.{pkPai} = c.{coluna} "
                + $"WHERE c.{coluna} IS NOT NULL AND p.{pkPai} IS NULL");
            Problema(sql, $"{tabela}.{coluna} vazio (obrigatório)", $"SELECT COUNT(*) FROM {Antigo}.{tabela} WHERE {coluna} IS NULL");
        }

        foreach (var p in e.GetProperties().Where(p => !p.IsShadowProperty() && !p.IsPrimaryKey() && !p.IsForeignKey()))
        {
            var coluna = p.Name;
            var nova = p.GetColumnName(t)!;
            var valor = ValorConvertido(e, p, $"{coluna}");
            if (!p.IsColumnNullable(t) && p.GetDefaultValueSql(t) is null)
                Problema(sql, $"{tabela}.{coluna} vazio (obrigatório)", $"SELECT COUNT(*) FROM {Antigo}.{tabela} WHERE {valor} IS NULL");
            if (p.GetMaxLength() is int tamanho)
                Problema(sql, $"{tabela}.{coluna} com mais de {tamanho} caracteres", $"SELECT COUNT(*) FROM {Antigo}.{tabela} WHERE CHAR_LENGTH({coluna}) > {tamanho}");
            var regra = e.GetCheckConstraints().FirstOrDefault(c => c.Sql.StartsWith(nova + " ", StringComparison.Ordinal));
            if (regra is not null)
                Problema(sql, $"{tabela}.{coluna} fora da regra {regra.Name}",
                    $"SELECT COUNT(*) FROM {Antigo}.{tabela} WHERE {valor} IS NOT NULL AND NOT ({Substituir(regra.Sql, nova, valor)})");
        }

        // Duplicidades que violariam um UNIQUE (associativas são deduplicadas na cópia; aqui só o que exige decisão).
        foreach (var indice in e.GetIndexes().Where(i => i.IsUnique && i.Properties.All(p => !p.IsForeignKey())))
        {
            if (tabela == "eixos") continue; // eixos com o mesmo nome são unificados no mapa
            var colunas = string.Join(", ", indice.Properties.Select(p => p.Name));
            Problema(sql, $"{tabela} com {colunas} repetido ({indice.GetDatabaseName()})",
                $"SELECT COUNT(*) FROM (SELECT {colunas} FROM {Antigo}.{tabela} GROUP BY {colunas} HAVING COUNT(*) > 1) d");
        }
    }

    private static void Problema(StringBuilder sql, string descricao, string contagem)
    {
        var i = contagem.IndexOf("COUNT(*)", StringComparison.Ordinal);
        var consulta = contagem[..i] + "COUNT(*) AS n" + contagem[(i + "COUNT(*)".Length)..];
        sql.Append($"INSERT INTO migracao_problemas SELECT '{descricao.Replace("'", "''")}', n FROM ({consulta}) x WHERE n > 0;\n");
    }

    // ── Mapas ─────────────────────────────────────────────────────────────

    private static void EscreverMapa(StringBuilder sql, IEntityType e)
    {
        var tabela = e.GetTableName()!;
        var pk = e.FindPrimaryKey()!.Properties[0].Name;
        var mapa = $"{Antigo}.migracao_mapa_{tabela}";
        sql.Append($"DROP TABLE IF EXISTS {mapa};\n");
        sql.Append($"CREATE TABLE {mapa} (antigo BINARY(16) NOT NULL PRIMARY KEY, novo INT UNSIGNED NOT NULL, primeiro BINARY(16) NOT NULL, KEY (novo));\n");
        if (tabela == "eixos")
        {
            // Eixos com o mesmo nome viram um só (UQ_Eixos_Nome); todos os Ids antigos apontam para ele.
            sql.Append($"""
                INSERT INTO {mapa} (antigo, novo, primeiro)
                SELECT e.IdEixo, DENSE_RANK() OVER (ORDER BY p.primeiro), p.primeiro
                FROM {Antigo}.eixos e
                JOIN (SELECT TRIM(Nome) AS nome, MIN(IdEixo) AS primeiro FROM {Antigo}.eixos GROUP BY TRIM(Nome)) p
                  ON p.nome = TRIM(e.Nome);

                """);
            return;
        }
        var criacao = e.GetProperties().FirstOrDefault(p => !p.IsShadowProperty() && p.GetColumnName() == "DtCriacao")?.Name;
        var ordem = criacao is null ? pk : $"{criacao}, {pk}";
        sql.Append($"INSERT INTO {mapa} (antigo, novo, primeiro) SELECT {pk}, ROW_NUMBER() OVER (ORDER BY {ordem}), {pk} FROM {Antigo}.{tabela};\n\n");
    }

    // ── Cópia ─────────────────────────────────────────────────────────────

    private static void EscreverCopia(StringBuilder sql, IEntityType e, HashSet<string> inseridas, List<(IEntityType, IForeignKey)> adiadas)
    {
        var tabela = e.GetTableName()!;
        var t = StoreObjectIdentifier.Table(tabela, null);
        var pk = e.FindPrimaryKey()!.Properties[0];
        var colunas = new List<string>();
        var valores = new List<string>();
        var juncoes = new List<string> { $"JOIN {Antigo}.migracao_mapa_{tabela} m ON m.antigo = o.{pk.Name}" };
        var indiceJuncao = 0;

        foreach (var p in e.GetProperties().Where(p => !p.IsShadowProperty()))
        {
            colunas.Add(p.GetColumnName(t)!);
            if (p.IsPrimaryKey())
            {
                valores.Add("m.novo");
                continue;
            }
            var fk = p.GetContainingForeignKeys().FirstOrDefault();
            if (fk is null)
            {
                // Eixos são unificados pelo nome sem espaços nas pontas (UQ_Eixos_Nome).
                valores.Add(tabela == "eixos" && p.Name == "Nome" ? "TRIM(o.Nome)" : ValorConvertido(e, p, $"o.{p.Name}"));
                continue;
            }
            var pai = fk.PrincipalEntityType.GetTableName()!;
            if (!inseridas.Contains(pai) && pai != tabela)
            {
                // Pai ainda não copiado (ciclo perfis <-> usuarios): grava NULL e preenche depois.
                valores.Add("NULL");
                adiadas.Add((e, fk));
                continue;
            }
            var apelido = $"f{++indiceJuncao}";
            if (tabela == "fontesinfo" && p.Name == "Fk_Id_Ator")
            {
                juncoes.Add($"LEFT JOIN {Antigo}.migracao_mapa_atores {apelido} ON {apelido}.antigo = o.AtorIdAtores");
                juncoes.Add($"LEFT JOIN {Antigo}.migracao_mapa_atores {apelido}b ON {apelido}b.antigo = o.Fk_Id_Ator");
                valores.Add($"COALESCE({apelido}.novo, {apelido}b.novo)");
                continue;
            }
            juncoes.Add($"LEFT JOIN {Antigo}.migracao_mapa_{pai} {apelido} ON {apelido}.antigo = o.{p.Name}");
            valores.Add($"{apelido}.novo");
        }

        var unicos = e.GetIndexes().Where(i => i.IsUnique).ToList();
        var corpo = $"SELECT {string.Join(", ", valores.Select((v, i) => $"{v} AS c{i}"))}\nFROM {Antigo}.{tabela} o\n{string.Join("\n", juncoes)}";
        if (tabela == "eixos")
            corpo += "\nWHERE o.IdEixo = m.primeiro";

        sql.Append($"INSERT INTO {Novo}.{tabela} ({string.Join(", ", colunas)})\n");
        if (unicos.Count == 0 || tabela is "eixos" or "usuarios")
        {
            sql.Append($"SELECT {string.Join(", ", colunas.Select((_, i) => $"c{i}"))} FROM (\n{corpo}\n) dados;\n\n");
            return;
        }

        // Associativas e permissões: se o mesmo par aparecer mais de uma vez, fica o primeiro.
        var particao = string.Join(", ", unicos[0].Properties.Select(p => $"c{colunas.IndexOf(p.GetColumnName(t)!)}"));
        sql.Append($"SELECT {string.Join(", ", colunas.Select((_, i) => $"c{i}"))} FROM (\n"
            + $"SELECT dados.*, ROW_NUMBER() OVER (PARTITION BY {particao} ORDER BY c0) AS repeticao FROM (\n{corpo}\n) dados\n"
            + ") unicos WHERE repeticao = 1;\n\n");
    }

    private static void EscreverAtualizacaoAdiada(StringBuilder sql, IEntityType e, IForeignKey fk)
    {
        var tabela = e.GetTableName()!;
        var t = StoreObjectIdentifier.Table(tabela, null);
        var coluna = fk.Properties[0];
        var pai = fk.PrincipalEntityType.GetTableName()!;
        var pk = e.FindPrimaryKey()!.Properties[0];
        sql.Append($"""
            UPDATE {Novo}.{tabela} n
            JOIN {Antigo}.migracao_mapa_{tabela} m ON m.novo = n.{pk.GetColumnName(t)}
            JOIN {Antigo}.{tabela} o ON o.{pk.Name} = m.antigo
            JOIN {Antigo}.migracao_mapa_{pai} p ON p.antigo = o.{coluna.Name}
            SET n.{coluna.GetColumnName(t)} = p.novo, n.DtModificacao = n.DtModificacao;


            """);
    }

    // ── Auxiliares ────────────────────────────────────────────────────────

    private static string ColunaAntiga(IEntityType e, IProperty p) => p.Name;

    // Valor antigo já no formato novo: Sim/Não -> S/N e datas sem horário quando a coluna é DATE.
    private static string ValorConvertido(IEntityType e, IProperty p, string coluna)
    {
        var tipo = p.GetColumnType();
        if (p.GetValueConverter() is { } conversor && conversor.ProviderClrType == typeof(string) && p.ClrType == typeof(string))
            return $"CASE {coluna} WHEN 'Sim' THEN 'S' WHEN 'Não' THEN 'N' WHEN '' THEN NULL ELSE {coluna} END";
        if (tipo == "date")
            return $"DATE({coluna})";
        return coluna;
    }

    private static string Substituir(string regra, string colunaNova, string valor) =>
        System.Text.RegularExpressions.Regex.Replace(regra, $@"\b{colunaNova}\b", valor);

    // Pais antes dos filhos; chaves opcionais que formam ciclo (perfis.IdUsuario) ficam para depois.
    private static List<IEntityType> OrdemDeInsercao(List<IEntityType> entidades)
    {
        var restantes = entidades.OrderBy(e => e.GetTableName(), StringComparer.Ordinal).ToList();
        var ordem = new List<IEntityType>();
        while (restantes.Count > 0)
        {
            var proxima = restantes.FirstOrDefault(e => e.GetForeignKeys()
                .Where(fk => !fk.Properties[0].IsNullable || fk.PrincipalEntityType == e)
                .All(fk => fk.PrincipalEntityType == e || ordem.Contains(fk.PrincipalEntityType)))
                ?? throw new InvalidOperationException("Ciclo de chaves obrigatórias no modelo.");
            // Entre as possíveis, prefere a que também já tem os pais opcionais copiados.
            var melhor = restantes.FirstOrDefault(e => e.GetForeignKeys()
                .All(fk => fk.PrincipalEntityType == e || ordem.Contains(fk.PrincipalEntityType))) ?? proxima;
            ordem.Add(melhor);
            restantes.Remove(melhor);
        }
        return ordem;
    }
}
