namespace Empodera.Models;

public static class ActorAggregateExtensions
{
    private static readonly string[] ResourceNames =
    {
        "RedePrimaria",
        "SeguridadeSocial",
        "Substancias",
        "Moradia",
        "Prevencao",
        "AssistenciaBasica",
        "Educacao",
        "Saude",
        "Ocupacao",
        "Lazer"
    };

    public static IReadOnlyList<string> CanonicalResourceNames => ResourceNames;

    public static void ConfigureCreationAggregate(
        this Atores actor,
        int communityId,
        IEnumerable<string>? resources,
        IEnumerable<string>? vulnerabilities)
    {
        ArgumentNullException.ThrowIfNull(actor);

        var selectedResources = (resources ?? Array.Empty<string>())
            .ToHashSet(StringComparer.Ordinal);
        var selectedVulnerabilities = (vulnerabilities ?? Array.Empty<string>())
            .ToHashSet(StringComparer.Ordinal);

        actor.Comunidades.Clear();
        actor.RecursosAtores.Clear();
        actor.Comunidades.Add(new AtorComunidade
        {
            Ator = actor,
            FkIdComunidade = communityId
        });

        foreach (var name in ResourceNames)
        {
            actor.RecursosAtores.Add(new RecursosAtores
            {
                Atores = actor,
                Nome = name,
                Tipo = "Recurso",
                Pode = selectedResources.Contains(name) ? "S" : "N"
            });
            actor.RecursosAtores.Add(new RecursosAtores
            {
                Atores = actor,
                Nome = name,
                Tipo = "Vulnerabilidade",
                Pode = selectedVulnerabilities.Contains(name) ? "S" : "N"
            });
        }
    }
}