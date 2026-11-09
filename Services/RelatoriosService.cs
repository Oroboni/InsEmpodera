using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Services;

// Dados dos relatórios por comunidade (tela, Excel e PDF usam os mesmos números).
public class RelatoriosService
{
    private readonly ApplicationDbContext _context;

    public RelatoriosService(ApplicationDbContext context) => _context = context;

    private static readonly (string Chave, string Nome)[] EixosDoAtor =
    [
        ("RedePrimaria", "Rede primária"), ("SeguridadeSocial", "Seguridade Social"), ("Substancias", "Substâncias"),
        ("Moradia", "Moradia"), ("Prevencao", "Prevenção"), ("AssistenciaBasica", "Assistência Básica"),
        ("Educacao", "Educação"), ("Saude", "Saúde"), ("Ocupacao", "Ocupação"), ("Lazer", "Lazer")
    ];

    public static string NomeGenero(int? genero) => genero switch
    {
        1 => "Masculino",
        2 => "Feminino",
        3 => "Transgênero",
        _ => "Não declarado"
    };

    public Task<string?> NomeComunidadeAsync(int? comunidadeId, CancellationToken ct) =>
        comunidadeId is null
            ? Task.FromResult<string?>(null)
            : _context.Comunidades.AsNoTracking().Where(c => c.Id_Comunidade == comunidadeId)
                .Select(c => (string?)c.Nome).FirstOrDefaultAsync(ct);

    private IQueryable<int> AtoresDaComunidade(int comunidadeId) =>
        _context.AtorComunidades.AsNoTracking()
            .Where(ac => ac.FkIdComunidade == comunidadeId && ac.Ator.Ativo != "N")
            .Select(ac => ac.FK_id_Atores)
            .Distinct();

    // ── Ações ─────────────────────────────────────────────────────────────

    public async Task<ActionsReportViewModel> AcoesAsync(
        int? comunidadeId, int? atorId, int? eixoId, DateTime? dataDe, DateTime? dataAte, CancellationToken ct)
    {
        var acoes = new List<DiarioDAcoes>();
        if (comunidadeId.HasValue)
        {
            var query = _context.DiarioDAcoes.AsNoTracking()
                .Include(acao => acao.Diario)
                .Include(acao => acao.DAtores).ThenInclude(link => link.Ator)
                .Include(acao => acao.Detalhes).ThenInclude(d => d.DetalhesEixos).ThenInclude(link => link.Eixo)
                .Where(acao => acao.Diario.FkIdComunidade == comunidadeId.Value);
            if (atorId.HasValue) query = query.Where(acao => acao.DAtores.Any(link => link.FK_id_Atores == atorId.Value));
            if (eixoId.HasValue) query = query.Where(acao => acao.Detalhes.Any(d => d.DetalhesEixos.Any(link => link.FkIdEixo == eixoId.Value)));
            if (dataDe.HasValue) query = query.Where(acao => acao.Diario.Data >= dataDe.Value.Date);
            if (dataAte.HasValue) query = query.Where(acao => acao.Diario.Data < dataAte.Value.Date.AddDays(1));
            acoes = await query.ToListAsync(ct);
        }

        static IEnumerable<string> Eixos(DiarioDAcoes acao) => acao.Detalhes
            .SelectMany(d => d.DetalhesEixos).Select(link => link.Eixo.Nome).Distinct().OrderBy(nome => nome);
        static int Atendidos(IEnumerable<DiarioDAcoes> lista) => lista
            .SelectMany(acao => acao.DAtores).Where(link => !link.Ator.DaEquipe).Select(link => link.FK_id_Atores).Distinct().Count();
        static int Envolvidos(IEnumerable<DiarioDAcoes> lista) => lista
            .SelectMany(acao => acao.DAtores).Select(link => link.FK_id_Atores).Distinct().Count();

        var eixoNome = eixoId is null ? null
            : await _context.Eixos.AsNoTracking().Where(e => e.IdEixo == eixoId).Select(e => e.Nome).FirstOrDefaultAsync(ct);

        var linhas = acoes
            .SelectMany(acao => Eixos(acao).Select(eixo => (Eixo: eixo, Acao: acao)))
            .Where(item => eixoNome is null || item.Eixo == eixoNome)
            .GroupBy(item => item.Eixo)
            .OrderBy(grupo => grupo.Key)
            .Select(grupo =>
            {
                var lista = grupo.Select(item => item.Acao).DistinctBy(acao => acao.IdDAcoes).ToList();
                return new ActionReportRow(grupo.Key, lista.Count, lista.Sum(acao => acao.Quantidade), Envolvidos(lista), Atendidos(lista));
            })
            .ToList();

        var detalhes = acoes
            .OrderBy(acao => acao.Diario.Data).ThenBy(acao => acao.Nome)
            .Select(acao => new ActionDetailRow(
                string.Join(", ", Eixos(acao)) is { Length: > 0 } eixos ? eixos : "Sem eixo",
                acao.Nome,
                acao.Diario.Data,
                Atendidos([acao]),
                acao.Quantidade,
                Envolvidos([acao])))
            .ToList();

        return new ActionsReportViewModel
        {
            ComunidadeId = comunidadeId,
            ComunidadeNome = await NomeComunidadeAsync(comunidadeId, ct),
            AtorId = atorId,
            EixoId = eixoId,
            EixoNome = eixoNome,
            DataDe = dataDe,
            DataAte = dataAte,
            Rows = linhas,
            Details = detalhes,
            TotalAttended = Atendidos(acoes),
            TotalActors = Envolvidos(acoes)
        };
    }

    // ── RSC ───────────────────────────────────────────────────────────────

    public async Task<RscReportViewModel> RscAsync(int? comunidadeId, CancellationToken ct)
    {
        if (comunidadeId is null)
            return new RscReportViewModel();

        var atores = AtoresDaComunidade(comunidadeId.Value);
        var vinculos = await _context.RedesPrimarias.AsNoTracking()
            .Where(r => r.TipoRede == TiposRede.Subjetiva && atores.Contains(r.FkIdAtorPrincipal) && r.AtorRelacionado.Ativo != "N")
            .Select(r => new
            {
                Ator1 = r.AtorPrincipal.Nome,
                Ator1Id = r.FkIdAtorPrincipal,
                Ator2Id = r.FkIdAtorRelacionados,
                r.TipoRelacao,
                Ator2 = r.AtorRelacionado
            })
            .ToListAsync(ct);

        var linhas = vinculos
            .OrderBy(v => v.Ator1).ThenBy(v => v.Ator2.Nome)
            .Select(v => new RscRow(
                v.Ator1, v.Ator2.Nome, v.TipoRelacao, NomeGenero(v.Ator2.Genero), v.Ator2.PapelSocial1, v.Ator2.PapelSocial2,
                v.Ator2.DaEquipe, v.Ator2.Rope || v.Ator2.OperadorPar, v.Ator2.Lopiniao, v.Ator2.Parceiro,
                v.Ator2.Mcomunidade, v.Ator2.RecursoRelacional))
            .ToList();

        return new RscReportViewModel
        {
            ComunidadeId = comunidadeId,
            ComunidadeNome = await NomeComunidadeAsync(comunidadeId, ct),
            Rows = linhas,
            Nos = vinculos.SelectMany(v => new[] { v.Ator1Id, v.Ator2Id }).Distinct().Count()
        };
    }

    // ── Rede Primária ─────────────────────────────────────────────────────

    public async Task<PrimaryNetworkReportViewModel> RedePrimariaAsync(int? comunidadeId, int? atorId, CancellationToken ct)
    {
        if (atorId is null)
            return new PrimaryNetworkReportViewModel { ComunidadeId = comunidadeId };

        // Comunidade do ator: a escolhida no filtro ou a primeira em que ele está.
        var comunidadeDoAtor = await _context.AtorComunidades.AsNoTracking()
            .Where(ac => ac.FK_id_Atores == atorId && (comunidadeId == null || ac.FkIdComunidade == comunidadeId))
            .OrderBy(ac => ac.IdAtorComunidade)
            .Select(ac => (int?)ac.FkIdComunidade)
            .FirstOrDefaultAsync(ct);

        var primaria = _context.RedesPrimarias.AsNoTracking()
            .Where(r => r.TipoRede == TiposRede.Primaria && r.AtorPrincipal.Ativo != "N" && r.AtorRelacionado.Ativo != "N");
        var saem = await primaria.Where(r => r.FkIdAtorPrincipal == atorId)
            .Select(r => new { r.FkIdAtorRelacionados, r.TipoRelacao })
            .ToListAsync(ct);
        var chegam = await primaria.CountAsync(r => r.FkIdAtorRelacionados == atorId, ct);

        var membros = saem.Select(r => r.FkIdAtorRelacionados).Distinct().ToList();
        var totalComunidade = comunidadeDoAtor is int idComunidade ? await AtoresDaComunidade(idComunidade).CountAsync(ct) : 0;

        var recursos = await _context.RecursosAtores.AsNoTracking()
            .Where(r => membros.Contains(r.FK_id_Atores) && r.Pode == "S")
            .Select(r => new { r.FK_id_Atores, r.Tipo, r.Nome })
            .ToListAsync(ct);
        decimal Proporcao(string tipo, string chave) => membros.Count == 0 ? 0
            : Math.Round(recursos.Where(r => r.Tipo == tipo && r.Nome == chave).Select(r => r.FK_id_Atores).Distinct().Count() / (decimal)membros.Count, 2);

        return new PrimaryNetworkReportViewModel
        {
            ComunidadeId = comunidadeId,
            AtorId = atorId,
            ComunidadeNome = await NomeComunidadeAsync(comunidadeDoAtor, ct),
            Amplitude = membros.Count,
            AtoresNaComunidade = totalComunidade,
            Densidade = totalComunidade == 0 ? null : Math.Round(membros.Count * 100m / totalComunidade, 2),
            Grau = saem.Count + chegam,
            Centralidade = totalComunidade <= 1 ? null : Math.Round((saem.Count + chegam) / (decimal)(totalComunidade - 1), 2),
            Relacoes = saem.GroupBy(r => r.TipoRelacao).OrderByDescending(g => g.Count()).ThenBy(g => g.Key)
                .Select(g => new ReportSeriesItem(g.Key, g.Count(), Math.Round(g.Count() * 100m / saem.Count, 1)))
                .ToList(),
            Eixos = EixosDoAtor.Select(e => new NetworkAxisShare(e.Nome, Proporcao("Recurso", e.Chave), Proporcao("Vulnerabilidade", e.Chave))).ToList()
        };
    }

    // ── Avaliações pessoais ───────────────────────────────────────────────

    public async Task<PersonalAssessmentReportViewModel> AvaliacoesAsync(
        int? comunidadeId, int? atorId, IReadOnlyCollection<int>? avaliacaoIds, CancellationToken ct)
    {
        if (atorId is null)
            return new PersonalAssessmentReportViewModel { ComunidadeId = comunidadeId };

        var avaliacoes = await _context.AvaliacaoPessoal.AsNoTracking()
            .Where(a => a.FK_id_Atores == atorId)
            .OrderByDescending(a => a.DtCriacao).ThenByDescending(a => a.IdAvaliacao)
            .ToListAsync(ct);
        var selecionadas = avaliacoes.Where(a => avaliacaoIds?.Contains(a.IdAvaliacao) == true).ToList();
        if (selecionadas.Count == 0)
            selecionadas = avaliacoes.Take(1).ToList();

        return new PersonalAssessmentReportViewModel
        {
            ComunidadeId = comunidadeId,
            AtorId = atorId,
            AtorNome = await _context.Atores.AsNoTracking().Where(a => a.IdAtores == atorId).Select(a => a.Nome).FirstOrDefaultAsync(ct),
            Avaliacoes = avaliacoes,
            // Da mais antiga para a mais recente: a ordem das cores acompanha o tempo.
            Selecionadas = selecionadas.OrderBy(a => a.DtCriacao).ThenBy(a => a.IdAvaliacao).ToList()
        };
    }

    // ── Relatório geral da comunidade ─────────────────────────────────────

    public async Task<CommunitySummaryViewModel> ResumoComunidadeAsync(int? comunidadeId, CancellationToken ct)
    {
        if (comunidadeId is null)
            return new CommunitySummaryViewModel();

        var id = comunidadeId.Value;
        var acoes = await AcoesAsync(id, null, null, null, null, ct);
        var vinculosAcoes = await _context.DAAtores.AsNoTracking()
            .Where(link => link.DiarioDAcoes.Diario.FkIdComunidade == id)
            .Select(link => new { link.FK_id_Atores, link.Ator.DaEquipe })
            .ToListAsync(ct);

        return new CommunitySummaryViewModel
        {
            ComunidadeId = id,
            ComunidadeNome = await NomeComunidadeAsync(id, ct),
            Atores = await AtoresDaComunidade(id).CountAsync(ct),
            Recursos = await _context.RedeRecursos.CountAsync(r => r.FkIdComunidade == id && (r.FK_id_Atores == null || r.Ator.Ativo != "N"), ct),
            Vulnerabilidades = await _context.Vulnerabilidades.CountAsync(v => v.FkIdComunidade == id, ct),
            Atividades = await _context.Atividades.CountAsync(a => a.FkIdComunidade == id, ct),
            Atendidos = vinculosAcoes.Where(v => !v.DaEquipe).Select(v => v.FK_id_Atores).Distinct().Count(),
            Atendimentos = acoes.TotalQuantity,
            AtoresEnvolvidos = vinculosAcoes.Select(v => v.FK_id_Atores).Distinct().Count()
        };
    }
}
