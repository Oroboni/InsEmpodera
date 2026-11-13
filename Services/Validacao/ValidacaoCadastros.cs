using Empodera.Data;
using Empodera.Models;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Services.Validacao;

// Regras de validação usadas por mais de uma tela (antes ficavam só no navegador ou estouravam erro 500 no banco).
public static class ValidacaoCadastros
{
    public static string Obrigatorio(string campo) =>
        string.Format(MensagensValidacao.Obrigatorio, ValidacaoAmigavelProvider.NomeDoCampo(campo));

    public static string Maximo(string campo, int tamanho) =>
        string.Format(MensagensValidacao.TamanhoMaximo, ValidacaoAmigavelProvider.NomeDoCampo(campo), tamanho);

    // Texto obrigatório: tira espaços das pontas e confere o tamanho.
    public static string? Texto(ModelStateDictionary modelState, string campo, string? valor, int maximo, bool obrigatorio = true)
    {
        var limpo = valor?.Trim();
        if (string.IsNullOrEmpty(limpo))
        {
            if (obrigatorio)
                modelState.AddModelError(campo, Obrigatorio(campo));
            return obrigatorio ? string.Empty : null;
        }
        if (limpo.Length > maximo)
            modelState.AddModelError(campo, Maximo(campo, maximo));
        return limpo;
    }

    public static async Task<bool> ComunidadeAtivaAsync(ApplicationDbContext context, ModelStateDictionary modelState, int? comunidadeId, string campo = "FkIdComunidade")
    {
        if (comunidadeId is int id && id > 0 && await context.Comunidades.AnyAsync(c => c.Id_Comunidade == id))
            return true;
        modelState.AddModelError(campo, Obrigatorio("FkIdComunidade"));
        return false;
    }

    // Atividade: nome, comunidade ativa e ao menos um eixo existente (a tela marca "Eixos *").
    public static async Task ValidarAtividadeAsync(ApplicationDbContext context, ModelStateDictionary modelState, Atividades atividade, int comunidadeId, List<int>? eixos)
    {
        // Só as regras abaixo valem: as navegações (Usuario, Comunidade) não vêm do formulário.
        modelState.Clear();
        atividade.Nome = Texto(modelState, nameof(Atividades.Nome), atividade.Nome, 150)!;
        atividade.Descricao = Texto(modelState, nameof(Atividades.Descricao), atividade.Descricao, 2000, obrigatorio: false);
        await ComunidadeAtivaAsync(context, modelState, comunidadeId);
        var ids = (eixos ?? []).Distinct().ToList();
        if (ids.Count == 0 || await context.Eixos.CountAsync(e => ids.Contains(e.IdEixo)) != ids.Count)
            modelState.AddModelError("EixosSelecionados", Obrigatorio("EixosSelecionados"));
    }
}
