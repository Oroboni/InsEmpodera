using MySqlConnector;

namespace InsEmpodera.Tests.Infrastructure;

/// <summary>
/// Cria o banco de teste executando o script oficial Database/Empodera.schema.sql
/// (o mesmo usado em produção), em vez de deixar o EF criar as tabelas. Assim todos os
/// testes também validam o DDL revisado.
/// </summary>
internal static class EsquemaMySql
{
    private static readonly Lazy<string> Script = new(() =>
    {
        var pasta = new DirectoryInfo(AppContext.BaseDirectory);
        while (pasta is not null && !File.Exists(Path.Combine(pasta.FullName, "InsEmpodera.csproj")))
            pasta = pasta.Parent;
        if (pasta is null)
            throw new InvalidOperationException("Raiz do repositório não encontrada para ler Database/Empodera.schema.sql.");
        return File.ReadAllText(Path.Combine(pasta.FullName, "Database", "Empodera.schema.sql"));
    });

    public static async Task CriarAsync(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder(connectionString);
        var banco = builder.Database;
        builder.Database = string.Empty;
        await using (var servidor = new MySqlConnection(builder.ConnectionString))
        {
            await servidor.OpenAsync();
            await using var criar = new MySqlCommand($"CREATE DATABASE `{banco}`", servidor);
            await criar.ExecuteNonQueryAsync();
        }

        await using var conexao = new MySqlConnection(connectionString);
        await conexao.OpenAsync();
        await using var comando = new MySqlCommand(Script.Value, conexao) { CommandTimeout = 120 };
        await comando.ExecuteNonQueryAsync();
    }

    public static void Criar(string connectionString) => CriarAsync(connectionString).GetAwaiter().GetResult();
}
