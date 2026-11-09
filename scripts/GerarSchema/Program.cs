// Gera Database/Empodera.schema.sql e Database/migracao-uuid-para-int.sql a partir do modelo do EF.
//   dotnet run --project scripts/GerarSchema               -> grava o arquivo
//   dotnet run --project scripts/GerarSchema -- --verificar -> só confere (sai com 1 se estiver desatualizado)
namespace GerarSchema;

internal static class Programa
{
    private static int Main(string[] args)
    {
        var raiz = new DirectoryInfo(AppContext.BaseDirectory);
        while (raiz is not null && !File.Exists(Path.Combine(raiz.FullName, "InsEmpodera.csproj")))
            raiz = raiz.Parent;
        if (raiz is null)
        {
            Console.Error.WriteLine("Não encontrei a raiz do repositório (InsEmpodera.csproj).");
            return 2;
        }

        var arquivos = new (string Caminho, string Conteudo)[]
        {
            (Path.Combine(raiz.FullName, "Database", "Empodera.schema.sql"), GeradorEsquema.Gerar()),
            (Path.Combine(raiz.FullName, "Database", "migracao-uuid-para-int.sql"), GeradorMigracao.Gerar())
        };

        if (args.Contains("--verificar"))
        {
            var desatualizados = arquivos
                .Where(a => !File.Exists(a.Caminho) || File.ReadAllText(a.Caminho).Replace("\r\n", "\n") != a.Conteudo)
                .Select(a => Path.GetFileName(a.Caminho))
                .ToList();
            if (desatualizados.Count == 0)
            {
                Console.WriteLine("Scripts do banco atualizados.");
                return 0;
            }
            Console.Error.WriteLine($"Desatualizados: {string.Join(", ", desatualizados)}. Rode: dotnet run --project scripts/GerarSchema");
            return 1;
        }

        foreach (var (caminho, conteudo) in arquivos)
        {
            File.WriteAllText(caminho, conteudo);
            Console.WriteLine($"Gerado: {caminho}");
        }
        return 0;
    }
}
