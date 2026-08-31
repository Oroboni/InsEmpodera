using Empodera.Data;
using Empodera.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Controllers;

public class RelatoriosController : BaseController
{
    private readonly ApplicationDbContext _context;

    public RelatoriosController(ILogger<AtoresController> logger, ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var denied = await RequireSerPermissionAsync(user => user.CanList("SER"));
        return denied ?? View();
    }

    public async Task<IActionResult> Rsc()
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null)
            return denied;

        await PopulateActiveCommunitiesAsync();
        return View();
    }

    public async Task<IActionResult> PrimaryNetwork()
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null)
            return denied;

        await PopulateActiveCommunitiesAsync();
        return View();
    }

    public async Task<IActionResult> PersonalAssessment()
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        return denied ?? View();
    }

    public async Task<IActionResult> FirstContact(
        int? comunidadeId,
        string campo = "status",
        int? genero = null,
        string? estruturaFamiliar = null,
        string? condicao = null,
        string? resultado = null,
        string? resposta = null,
        string? horario = null,
        string? comoEstabeleceu = null,
        string? escolaridade = null,
        DateTime? dataDe = null,
        DateTime? dataAte = null)
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null)
            return denied;

        await PopulateActiveCommunitiesAsync(comunidadeId);
        var records = new List<FichaPrimeiroContato>();
        if (comunidadeId.HasValue)
        {
            var query = _context.FichasPrimeiroContato.AsNoTracking()
                .Include(f => f.Ator)
                .Include(f => f.FichaCondicoes)
                .Include(f => f.FichaRespostas)
                .Include(f => f.FichaResultados)
                .Where(f => f.FkIdComunidade == comunidadeId.Value);
            if (genero.HasValue) query = query.Where(f => f.Ator.Genero == genero.Value);
            if (!string.IsNullOrWhiteSpace(estruturaFamiliar)) query = query.Where(f => f.EstruFamiliar == estruturaFamiliar);
            if (!string.IsNullOrWhiteSpace(condicao)) query = query.Where(f => f.FichaCondicoes!.Any(c => c.Cond == condicao));
            if (!string.IsNullOrWhiteSpace(resultado)) query = query.Where(f => f.FichaResultados!.Any(r => r.Result == resultado));
            if (!string.IsNullOrWhiteSpace(resposta)) query = query.Where(f => f.FichaRespostas!.Any(r => r.Resp == resposta));
            if (!string.IsNullOrWhiteSpace(comoEstabeleceu)) query = query.Where(f => f.CEstabeleceu == comoEstabeleceu);
            if (dataDe.HasValue) query = query.Where(f => f.DtContato >= dataDe.Value.Date);
            if (dataAte.HasValue) query = query.Where(f => f.DtContato < dataAte.Value.Date.AddDays(1));
            records = await query.ToListAsync(HttpContext.RequestAborted);
            records = records.Where(f => MatchesTime(f.HoraContato, horario) && MatchesSchooling(f, escolaridade)).ToList();
        }

        var grouped = records
            .GroupBy(f => GetFirstContactGroup(f, campo))
            .OrderByDescending(group => group.Count())
            .ThenBy(group => group.Key)
            .Select(group => new ReportSeriesItem(
                group.Key,
                group.Count(),
                records.Count == 0 ? 0 : Math.Round(group.Count() * 100m / records.Count, 1)))
            .ToList();

        return View(new FirstContactReportViewModel
        {
            ComunidadeId = comunidadeId,
            Campo = campo,
            Genero = genero,
            EstruturaFamiliar = estruturaFamiliar,
            Condicao = condicao,
            Resultado = resultado,
            Resposta = resposta,
            Horario = horario,
            ComoEstabeleceu = comoEstabeleceu,
            Escolaridade = escolaridade,
            DataDe = dataDe,
            DataAte = dataAte,
            Total = records.Count,
            Series = grouped
        });
    }

    public async Task<IActionResult> Actions(int? comunidadeId, int? atorId, DateTime? dataDe, DateTime? dataAte)
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null)
            return denied;

        await PopulateActiveCommunitiesAsync(comunidadeId);
        ViewBag.AtorList = new SelectList(
            await _context.Atores.AsNoTracking()
                .Where(actor => actor.Ativo == "S"
                    && (!comunidadeId.HasValue || actor.Comunidades.Any(link => link.FkIdComunidade == comunidadeId.Value)))
                .OrderBy(actor => actor.Nome).ToListAsync(HttpContext.RequestAborted),
            "IdAtores", "Nome", atorId);

        var actions = new List<DiarioDAcoes>();
        if (comunidadeId.HasValue)
        {
            var query = _context.DiarioDAcoes.AsNoTracking()
                .Include(action => action.Diario)
                .Include(action => action.DAtores)
                .Include(action => action.Detalhes)
                .ThenInclude(detail => detail.DetalhesEixos)
                .ThenInclude(link => link.Eixo)
                .Where(action => action.Diario.FkIdComunidade == comunidadeId.Value);
            if (atorId.HasValue) query = query.Where(action => action.DAtores.Any(link => link.FK_id_Atores == atorId.Value));
            if (dataDe.HasValue) query = query.Where(action => action.Diario.Data >= dataDe.Value.Date);
            if (dataAte.HasValue) query = query.Where(action => action.Diario.Data < dataAte.Value.Date.AddDays(1));
            actions = await query.ToListAsync(HttpContext.RequestAborted);
        }

        var rows = actions
            .SelectMany(action => action.Detalhes
                .SelectMany(detail => detail.DetalhesEixos)
                .Select(link => new { Action = action, Axis = link.Eixo.Nome }))
            .GroupBy(item => item.Axis)
            .OrderBy(group => group.Key)
            .Select(group => new ActionReportRow(
                group.Key,
                group.Select(item => item.Action.IdDAcoes).Distinct().Count(),
                group.GroupBy(item => item.Action.IdDAcoes).Sum(items => items.First().Action.Quantidade),
                group.SelectMany(item => item.Action.DAtores).Select(link => link.FK_id_Atores).Distinct().Count()))
            .ToList();

        return View(new ActionsReportViewModel
        {
            ComunidadeId = comunidadeId,
            AtorId = atorId,
            DataDe = dataDe,
            DataAte = dataAte,
            Rows = rows
        });
    }

    private async Task<IActionResult?> RequireSerPermissionAsync(Func<Usuario?, bool> permission)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var loggedUserId = int.Parse(HttpContext.Session.GetString("ID") ?? "0");
        var loggedUser = await _context.Usuarios
            .AsNoTracking()
            .Include(user => user.Perfil)
            .ThenInclude(profile => profile.Permissoes)
            .FirstOrDefaultAsync(
                user => user.IdUsuario == loggedUserId,
                HttpContext.RequestAborted);

        return permission(loggedUser)
            ? null
            : RedirectToAction("Index", "Home");
    }

    private async Task PopulateActiveCommunitiesAsync(int? selectedId = null)
    {
        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades
                .AsNoTracking()
                .Where(community => community.Ativo != "N")
                .OrderBy(community => community.Nome)
                .ToListAsync(HttpContext.RequestAborted),
            "Id_Comunidade",
            "Nome",
            selectedId);
    }

    private static bool MatchesTime(TimeSpan time, string? period) => period switch
    {
        "06_12" => time >= TimeSpan.FromHours(6) && time <= TimeSpan.FromHours(12),
        "12_20" => time > TimeSpan.FromHours(12) && time <= TimeSpan.FromHours(20),
        "20_06" => time > TimeSpan.FromHours(20) || time < TimeSpan.FromHours(6),
        _ => true
    };

    private static bool MatchesSchooling(FichaPrimeiroContato record, string? schooling) => schooling switch
    {
        "sabe_calcular" => record.SCalc == "Sim",
        "sabe_ler_escrever" => record.SLer == "Sim",
        "sabe_usar_computador" => record.SComp == "Sim",
        _ => true
    };

    private static string GetFirstContactGroup(FichaPrimeiroContato record, string field) => field switch
    {
        "como_estabeleceu_contato" => Label(record.CEstabeleceu),
        "emprego" => Label(record.Emprego),
        "estado_familiar" => Label(record.EstaFamiliar),
        "estrutura_familiar" => Label(record.EstruFamiliar),
        "genero" => record.Ator.Genero switch { 1 => "Masculino", 2 => "Feminino", 3 => "Transgênero", _ => "Não declarado" },
        "hora_primeiro_contato" => record.HoraContato < TimeSpan.FromHours(6) ? "20:01–05:59"
            : record.HoraContato <= TimeSpan.FromHours(12) ? "06:00–12:00"
            : record.HoraContato <= TimeSpan.FromHours(20) ? "12:01–20:00" : "20:01–05:59",
        "local_tratamento" => Label(record.LTrat),
        _ => record.Status switch { "Concluida" => "Concluída", "Abandonada" => "Abandonada", _ => "Em progresso" }
    };

    private static string Label(string? value) => string.IsNullOrWhiteSpace(value) ? "Não informado" : value;
}
