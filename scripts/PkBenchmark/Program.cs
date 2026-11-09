using System.Diagnostics;
using System.Text;
using MySqlConnector;

const int Trials = 3;
const int Reads = 600;
const int Joins = 200;
const int Mutations = 100;

var server = Environment.GetEnvironmentVariable("PK_BENCHMARK_MYSQL_CONNECTION")
    ?? throw new InvalidOperationException("Defina PK_BENCHMARK_MYSQL_CONNECTION para um MariaDB de teste.");
var sourceName = Environment.GetEnvironmentVariable("PK_BENCHMARK_SOURCE_DATABASE") ?? "empodera";
if (!SafeName(sourceName))
    throw new InvalidOperationException("Nome da base de origem inválido.");

var sourceBuilder = new MySqlConnectionStringBuilder(server) { Database = sourceName };
await using var source = new MySqlConnection(sourceBuilder.ConnectionString);
await source.OpenAsync();
var counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
await using (var tables = new MySqlCommand(
    "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = @schema AND TABLE_TYPE = 'BASE TABLE'",
    source))
{
    tables.Parameters.AddWithValue("@schema", sourceName);
    var names = new List<string>();
    await using (var reader = await tables.ExecuteReaderAsync())
        while (await reader.ReadAsync())
            names.Add(reader.GetString(0));
    foreach (var name in names)
    {
        if (!SafeName(name))
            throw new InvalidOperationException($"Nome de tabela inesperado: {name}");
        await using var count = new MySqlCommand($"SELECT COUNT(*) FROM {name}", source);
        counts[name] = checked((int)Convert.ToInt64(await count.ExecuteScalarAsync()));
    }
}
var total = counts.Values.Sum();
var actors = counts.GetValueOrDefault("atores");
var forms = counts.GetValueOrDefault("fichasprimeirocontato");
if (total < 2 || actors < 1 || actors >= total)
    throw new InvalidOperationException("O banco de origem não contém volume suficiente de atores e outros registros.");
await using var keyTypeCommand = new MySqlCommand(
    "SELECT COLUMN_TYPE FROM information_schema.COLUMNS WHERE TABLE_SCHEMA=@schema AND TABLE_NAME='atores' AND COLUMN_NAME='IdAtores'",
    source);
keyTypeCommand.Parameters.AddWithValue("@schema", sourceName);
var actualKeyType = Convert.ToString(await keyTypeCommand.ExecuteScalarAsync()) ?? "desconhecido";

var benchmarkName = $"insempodera_pkbench_{Guid.NewGuid():N}";
var adminBuilder = new MySqlConnectionStringBuilder(server) { Database = "" };
await using var admin = new MySqlConnection(adminBuilder.ConnectionString);
await admin.OpenAsync();
await Exec(admin, $"CREATE DATABASE {benchmarkName} CHARACTER SET utf8mb4");
var measurements = new List<Measurement>();
try
{
    var benchmarkBuilder = new MySqlConnectionStringBuilder(server) { Database = benchmarkName };
    await using var db = new MySqlConnection(benchmarkBuilder.ConnectionString);
    await db.OpenAsync();
    foreach (var scale in new[] { 1, 10, 25 })
    {
        var parentCount = checked(actors * scale);
        var childCount = checked((total - actors) * scale);
        for (var trial = 1; trial <= Trials; trial++)
        {
            var order = (trial % 3) switch
            {
                1 => new[] { KeyKind.GuidText, KeyKind.AutoIncrement, KeyKind.GuidBinary },
                2 => new[] { KeyKind.AutoIncrement, KeyKind.GuidBinary, KeyKind.GuidText },
                _ => new[] { KeyKind.GuidBinary, KeyKind.GuidText, KeyKind.AutoIncrement }
            };
            foreach (var kind in order)
            {
                Console.WriteLine($"{scale}x, {kind}, rodada {trial}/{Trials}: {parentCount + childCount:N0} linhas");
                measurements.Add(await Run(db, kind, scale, trial, parentCount, childCount));
            }
        }
    }
}
finally
{
    if (benchmarkName.StartsWith("insempodera_pkbench_", StringComparison.Ordinal))
        await Exec(admin, $"DROP DATABASE {benchmarkName}");
}

var report = new StringBuilder();
report.AppendLine("# Benchmark de PK — InsEmpodera");
report.AppendLine();
report.AppendLine($"MariaDB {source.ServerVersion}. Origem: {sourceName}, consultada somente para contar linhas.");
report.AppendLine($"Volume exato: {total:N0} linhas em {counts.Count} tabelas; atores: {actors:N0}; fichas: {forms:N0}.");
report.AppendLine($"PK em uso em atores.IdAtores: {actualKeyType}. O código C# usa Guid e configura GuidFormat.Binary16; portanto, GuidBinary representa a opção atual quando o tipo da coluna é binary(16).");
report.AppendLine($"Modelo de carga: {actors:N0} pais e {total - actors:N0} filhos na escala 1x, com índice de FK e cascata.");
report.AppendLine($"Cada cenário teve {Trials} rodadas. Os números abaixo são medianas em milissegundos.");
report.AppendLine();
report.AppendLine("| Escala | PK | Linhas | Inserção | 600 buscas PK | 200 joins FK | 100 updates | 100 deletes | Dados+índices KiB |");
report.AppendLine("|---:|---|---:|---:|---:|---:|---:|---:|---:|");
foreach (var scale in new[] { 1, 10, 25 })
foreach (var kind in Enum.GetValues<KeyKind>())
{
    var values = measurements.Where(m => m.Scale == scale && m.Kind == kind).ToArray();
    report.AppendLine($"| {scale}x | {kind} | {values[0].Rows:N0} | {Median(values.Select(x => x.Insert)):N1} | {Median(values.Select(x => x.Read)):N1} | {Median(values.Select(x => x.Join)):N1} | {Median(values.Select(x => x.Update)):N1} | {Median(values.Select(x => x.Delete)):N1} | {Median(values.Select(x => (double)x.Bytes)) / 1024:N1} |");
}
report.AppendLine();
report.AppendLine("GuidText = UUID v4 em CHAR(36); GuidBinary = UUID v4 em BINARY(16); AutoIncrement = INT AUTO_INCREMENT.");
report.AppendLine("A inserção inclui geração das chaves UUID no cliente; as buscas e mutações usam comandos parametrizados repetidos, com cache aquecido.");
report.AppendLine("Os dados sintéticos usam o total e a proporção de atores reais, mas não representam as 34 estruturas, todos os campos ou usuários simultâneos.");
report.AppendLine("O espaço de tabelas pequenas é arredondado em páginas InnoDB; não interprete pequenas diferenças de KiB como economia precisa.");
var binary25 = measurements.Where(m => m.Scale == 25 && m.Kind == KeyKind.GuidBinary).ToArray();
var auto25 = measurements.Where(m => m.Scale == 25 && m.Kind == KeyKind.AutoIncrement).ToArray();
var insertionDelta = 100 * (Median(binary25.Select(m => m.Insert)) / Median(auto25.Select(m => m.Insert)) - 1);
var spaceDelta = 100 * (Median(binary25.Select(m => (double)m.Bytes)) / Median(auto25.Select(m => (double)m.Bytes)) - 1);
report.AppendLine();
report.AppendLine($"Na escala 25x, a PK atual levou {insertionDelta:N1}% mais tempo para inserir e ocupou {spaceDelta:N1}% mais espaço de dados+índices que INT AUTO_INCREMENT neste modelo. Tempos de leitura, edição e exclusão ficaram próximos e variaram entre rodadas; não há evidência aqui de vantagem estável em todas as operações.");
report.AppendLine("Este benchmark mede custo técnico, não benefício funcional: a escolha de PK aleatória pode continuar válida para o projeto. Não é uma migração automática nem prova de comportamento com o esquema completo ou sob concorrência.");
var output = Path.GetFullPath(Path.Combine("TestResults", "PK-BENCHMARK.md"));
Directory.CreateDirectory(Path.GetDirectoryName(output)!);
await File.WriteAllTextAsync(output, report.ToString(), new UTF8Encoding(false));
Console.WriteLine(report);
Console.WriteLine($"Relatório: {output}");

static async Task<Measurement> Run(MySqlConnection db, KeyKind kind, int scale, int trial, int parents, int children)
{
    var suffix = $"{kind.ToString().ToLowerInvariant()}_{scale}_{trial}";
    var p = $"pkb_p_{suffix}";
    var c = $"pkb_c_{suffix}";
    var type = kind switch
    {
        KeyKind.GuidText => "CHAR(36) CHARACTER SET ascii COLLATE ascii_bin",
        KeyKind.GuidBinary => "BINARY(16)",
        _ => "INT"
    };
    var auto = kind == KeyKind.AutoIncrement ? " AUTO_INCREMENT" : "";
    await Exec(db, $"CREATE TABLE {p} (id {type} NOT NULL{auto}, payload VARCHAR(128) NOT NULL, PRIMARY KEY(id)) ENGINE=InnoDB");
    await Exec(db, $"CREATE TABLE {c} (id {type} NOT NULL{auto}, parent_id {type} NOT NULL, payload VARCHAR(192) NOT NULL, PRIMARY KEY(id), INDEX ix_parent_id(parent_id), CONSTRAINT fk_{suffix} FOREIGN KEY(parent_id) REFERENCES {p}(id) ON DELETE CASCADE) ENGINE=InnoDB");
    try
    {
        var keys = new object[parents];
        var timer = Stopwatch.StartNew();
        await using (var transaction = await db.BeginTransactionAsync())
        {
            var parentSql = kind == KeyKind.AutoIncrement
                ? $"INSERT INTO {p}(payload) VALUES (@payload)"
                : $"INSERT INTO {p}(id,payload) VALUES (@id,@payload)";
            await using var addParent = new MySqlCommand(parentSql, db, transaction);
            addParent.Parameters.AddWithValue("@payload", new string('P', 96));
            if (kind != KeyKind.AutoIncrement) addParent.Parameters.AddWithValue("@id", DBNull.Value);
            for (var i = 0; i < parents; i++)
            {
                if (kind != KeyKind.AutoIncrement) addParent.Parameters["@id"].Value = NewKey(kind);
                await addParent.ExecuteNonQueryAsync();
                keys[i] = kind == KeyKind.AutoIncrement
                    ? checked((int)addParent.LastInsertedId)
                    : addParent.Parameters["@id"].Value!;
            }
            var childSql = kind == KeyKind.AutoIncrement
                ? $"INSERT INTO {c}(parent_id,payload) VALUES (@parent,@payload)"
                : $"INSERT INTO {c}(id,parent_id,payload) VALUES (@id,@parent,@payload)";
            await using var addChild = new MySqlCommand(childSql, db, transaction);
            addChild.Parameters.AddWithValue("@parent", DBNull.Value);
            addChild.Parameters.AddWithValue("@payload", new string('C', 160));
            if (kind != KeyKind.AutoIncrement) addChild.Parameters.AddWithValue("@id", DBNull.Value);
            for (var i = 0; i < children; i++)
            {
                addChild.Parameters["@parent"].Value = keys[i % parents];
                if (kind != KeyKind.AutoIncrement) addChild.Parameters["@id"].Value = NewKey(kind);
                await addChild.ExecuteNonQueryAsync();
            }
            await transaction.CommitAsync();
        }
        timer.Stop();
        var insert = timer.Elapsed.TotalMilliseconds;

        await using var read = new MySqlCommand($"SELECT payload FROM {p} WHERE id=@id", db);
        read.Parameters.AddWithValue("@id", DBNull.Value);
        var random = new Random(1729);
        timer.Restart();
        for (var i = 0; i < Reads; i++)
        {
            read.Parameters["@id"].Value = keys[random.Next(parents)];
            if (await read.ExecuteScalarAsync() is not string)
                throw new InvalidOperationException("Busca por PK falhou.");
        }
        timer.Stop();
        var readMs = timer.Elapsed.TotalMilliseconds;

        await using var join = new MySqlCommand($"SELECT COUNT(*) FROM {p} p JOIN {c} c ON c.parent_id=p.id WHERE p.id=@id", db);
        join.Parameters.AddWithValue("@id", DBNull.Value);
        timer.Restart();
        for (var i = 0; i < Joins; i++)
        {
            join.Parameters["@id"].Value = keys[random.Next(parents)];
            if (Convert.ToInt64(await join.ExecuteScalarAsync()) < 1)
                throw new InvalidOperationException("Join por FK falhou.");
        }
        timer.Stop();
        var joinMs = timer.Elapsed.TotalMilliseconds;

        await Exec(db, $"ANALYZE TABLE {p}");
        await Exec(db, $"ANALYZE TABLE {c}");
        await using var size = new MySqlCommand(
            "SELECT COALESCE(SUM(DATA_LENGTH+INDEX_LENGTH),0) FROM information_schema.TABLES WHERE TABLE_SCHEMA=DATABASE() AND TABLE_NAME IN (@p,@c)", db);
        size.Parameters.AddWithValue("@p", p);
        size.Parameters.AddWithValue("@c", c);
        var bytes = Convert.ToInt64(await size.ExecuteScalarAsync());

        await using var update = new MySqlCommand($"UPDATE {p} SET payload=@payload WHERE id=@id", db);
        update.Parameters.AddWithValue("@payload", new string('U', 96));
        update.Parameters.AddWithValue("@id", DBNull.Value);
        timer.Restart();
        for (var i = 0; i < Mutations; i++)
        {
            update.Parameters["@id"].Value = keys[i % parents];
            if (await update.ExecuteNonQueryAsync() != 1)
                throw new InvalidOperationException("Update por PK falhou.");
        }
        timer.Stop();
        var updateMs = timer.Elapsed.TotalMilliseconds;

        await using var delete = new MySqlCommand($"DELETE FROM {p} WHERE id=@id", db);
        delete.Parameters.AddWithValue("@id", DBNull.Value);
        timer.Restart();
        for (var i = 0; i < Mutations; i++)
        {
            delete.Parameters["@id"].Value = keys[i % parents];
            if (await delete.ExecuteNonQueryAsync() != 1)
                throw new InvalidOperationException("Delete por PK falhou.");
        }
        timer.Stop();
        var deleteMs = timer.Elapsed.TotalMilliseconds;
        await using var count = new MySqlCommand($"SELECT COUNT(*) FROM {c}", db);
        var expected = children - Enumerable.Range(0, children).Count(i => i % parents < Mutations);
        if (Convert.ToInt32(await count.ExecuteScalarAsync()) != expected)
            throw new InvalidOperationException("A FK não aplicou a cascata esperada.");
        return new Measurement(kind, scale, parents + children, insert, readMs, joinMs, updateMs, deleteMs, bytes);
    }
    finally
    {
        await Exec(db, $"DROP TABLE IF EXISTS {c}");
        await Exec(db, $"DROP TABLE IF EXISTS {p}");
    }
}

static object NewKey(KeyKind kind)
{
    var id = Guid.NewGuid();
    return kind == KeyKind.GuidBinary ? id.ToByteArray() : id.ToString("D");
}
static bool SafeName(string value) => value.Length is > 0 and < 65 && value.All(c => char.IsAsciiLetterOrDigit(c) || c == '_');
static double Median(IEnumerable<double> values) => values.Order().ElementAt(Trials / 2);
static async Task Exec(MySqlConnection db, string sql)
{
    await using var command = new MySqlCommand(sql, db);
    await command.ExecuteNonQueryAsync();
}
enum KeyKind { GuidText, GuidBinary, AutoIncrement }
record Measurement(KeyKind Kind, int Scale, int Rows, double Insert, double Read, double Join, double Update, double Delete, long Bytes);
