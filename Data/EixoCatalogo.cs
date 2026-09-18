using Empodera.Models;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Data;

public static class EixoCatalogo
{
    public static readonly string[] Nomes =
    [
        "REDES",
        "SEGURIDADE SOCIAL",
        "SUBSTÂNCIAS",
        "MORADIA",
        "PREVENÇÃO",
        "ASSISTÊNCIA BÁSICA E REDUÇÃO DE DANOS",
        "EDUCAÇÃO",
        "SAÚDE FÍSICA E PSICOLÓGICA",
        "OCUPAÇÃO/TRABALHO",
        "LAZER/CULTURA"
    ];

    public static async Task EnsureCreatedAsync(ApplicationDbContext db)
    {
        var existentes = (await db.Eixos.AsNoTracking().Select(e => e.Nome).ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var nome in Nomes.Where(nome => !existentes.Contains(nome)))
            db.Eixos.Add(new Eixo { Nome = nome });
        await db.SaveChangesAsync();
    }

    public static async Task<List<Eixo>> ListarDisponiveisAsync(ApplicationDbContext db)
    {
        var nomes = Nomes;
        var padrao = await db.Eixos.AsNoTracking()
            .Where(e => nomes.Contains(e.Nome.ToUpper()))
            .OrderBy(e => e.Nome)
            .ToListAsync();

        // Bases de teste criadas sem o catálogo ainda podem usar seus próprios eixos.
        return padrao.Count > 0 ? padrao : await db.Eixos.AsNoTracking()
            .OrderBy(e => e.Nome).ToListAsync();
    }
}
