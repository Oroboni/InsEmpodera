using Empodera.Models;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Data;

// Tipos de recurso da comunidade (RedeRecursos.Tipo). Recursos antigos podem ter outro texto livre.
public static class TiposRecurso
{
    public const string Estrutural = "Estrutural";
    public const string Relacional = "Relacional";
    public const string Simbolico = "Simbólico";

    public static readonly IReadOnlyList<(string Valor, string Descricao)> Opcoes =
    [
        (Estrutural, "Estrutural (recursos materiais)"),
        (Relacional, "Relacional (pessoas)"),
        (Simbolico, "Simbólico (sentimentos, características)")
    ];

    public static bool EhValido(string? tipo) => tipo is Estrutural or Relacional or Simbolico;

    // Ator marcado como "Recurso relacional" entra nos recursos relacionais da comunidade (uma vez só).
    public static async Task GarantirRecursoRelacionalAsync(ApplicationDbContext context, Atores ator, int comunidadeId, int usuarioId)
    {
        if (!ator.RecursoRelacional)
            return;
        if (ator.IdAtores != 0 && await context.RedeRecursos.AnyAsync(r =>
                r.FkIdComunidade == comunidadeId && r.FK_id_Atores == ator.IdAtores && r.Tipo == Relacional))
            return;

        context.RedeRecursos.Add(new RedeRecursos
        {
            Ator = ator,
            FkIdComunidade = comunidadeId,
            Tipo = Relacional,
            Nome = ator.Nome,
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now,
            FkIdUsuario = usuarioId
        });
    }
}

public static class RedesDoAtorExtensions
{
    // Copia as marcações de rede do formulário para o ator gravado.
    public static void CopiarRedesDe(this Atores destino, Atores origem)
    {
        destino.DaEquipe = origem.DaEquipe;
        destino.Rope = origem.Rope;
        destino.Lopiniao = origem.Lopiniao;
        destino.Parceiro = origem.Parceiro;
        destino.Mcomunidade = origem.Mcomunidade;
        destino.OperadorPar = origem.OperadorPar;
        destino.RecursoRelacional = origem.RecursoRelacional;
    }
}
