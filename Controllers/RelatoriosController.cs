using Empodera.Data;
using Empodera.Models;
using Empodera.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Controllers;

public class RelatoriosController : BaseController
{
    private readonly ApplicationDbContext _context;
    private readonly RelatoriosService _relatorios;
    private readonly RelatorioArquivosService _arquivos;

    private const string TipoExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    public RelatoriosController(ApplicationDbContext context, RelatoriosService relatorios, RelatorioArquivosService arquivos)
    {
        _context = context;
        _relatorios = relatorios;
        _arquivos = arquivos;
    }

    public async Task<IActionResult> Index()
    {
        var denied = await RequireSerPermissionAsync(user => user.CanList("SER"));
        return denied ?? View();
    }

    public async Task<IActionResult> Rsc(int? comunidadeId, string? exportar)
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null)
            return denied;

        var model = await _relatorios.RscAsync(comunidadeId, HttpContext.RequestAborted);
        if (exportar == "excel" && model.ComunidadeId.HasValue)
            return File(_arquivos.RscExcel(model), TipoExcel, NomeArquivo("rsc", model.ComunidadeNome, "xlsx"));

        await PopulateActiveCommunitiesAsync(comunidadeId);
        return View(model);
    }

    public async Task<IActionResult> PrimaryNetwork(int? comunidadeId, int? atorId)
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null)
            return denied;

        await PopulateActiveCommunitiesAsync(comunidadeId);
        await PopulateActiveActorsAsync(atorId, comunidadeId);
        return View(await _relatorios.RedePrimariaAsync(comunidadeId, atorId, HttpContext.RequestAborted));
    }

    public async Task<IActionResult> PersonalAssessment(int? comunidadeId, int? atorId, List<int>? avaliacaoIds, int? avaliacaoId, string? exportar)
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null) return denied;

        // avaliacaoId: link antigo com uma só data.
        if (avaliacaoId.HasValue)
            (avaliacaoIds ??= []).Add(avaliacaoId.Value);
        var model = await _relatorios.AvaliacoesAsync(comunidadeId, atorId, avaliacaoIds, HttpContext.RequestAborted);
        if (model.Selecionadas.Count > 0 && exportar is "excel" or "pdf")
            return exportar == "excel"
                ? File(_arquivos.AvaliacoesExcel(model), TipoExcel, NomeArquivo("avaliacoes", model.AtorNome, "xlsx"))
                : File(_arquivos.AvaliacoesPdf(model), "application/pdf", NomeArquivo("avaliacoes", model.AtorNome, "pdf"));

        await PopulateActiveCommunitiesAsync(comunidadeId);
        await PopulateActiveActorsAsync(atorId, comunidadeId);
        return View(model);
    }

    public async Task<IActionResult> Community(int? comunidadeId, string? exportar)
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null)
            return denied;

        var model = await _relatorios.ResumoComunidadeAsync(comunidadeId, HttpContext.RequestAborted);
        if (exportar == "pdf" && model.ComunidadeId.HasValue)
            return File(_arquivos.ResumoComunidadePdf(model), "application/pdf", NomeArquivo("relatorio-geral", model.ComunidadeNome, "pdf"));

        await PopulateActiveCommunitiesAsync(comunidadeId);
        return View(model);
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
        if (comunidadeId.HasValue)
        {
            // Opções dos filtros: os valores que existem nas fichas da comunidade (o filtro compara o texto exato).
            var fichas = _context.FichasPrimeiroContato.AsNoTracking().Where(f => f.FkIdComunidade == comunidadeId.Value);
            ViewBag.OpcoesCondicao = await fichas.SelectMany(f => f.FichaCondicoes!).Select(c => c.Cond)
                .Distinct().OrderBy(v => v).ToListAsync(HttpContext.RequestAborted);
            ViewBag.OpcoesResposta = await fichas.SelectMany(f => f.FichaRespostas!).Select(r => r.Resp)
                .Distinct().OrderBy(v => v).ToListAsync(HttpContext.RequestAborted);
            ViewBag.OpcoesResultado = await fichas.SelectMany(f => f.FichaResultados!).Select(r => r.Result)
                .Distinct().OrderBy(v => v).ToListAsync(HttpContext.RequestAborted);
        }
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

    public async Task<IActionResult> Actions(int? comunidadeId, int? atorId, int? eixoId, DateTime? dataDe, DateTime? dataAte, string? exportar)
    {
        var denied = await RequireSerPermissionAsync(user => user.CanViewDetails("SER"));
        if (denied is not null)
            return denied;

        var model = await _relatorios.AcoesAsync(comunidadeId, atorId, eixoId, dataDe, dataAte, HttpContext.RequestAborted);
        if (model.ComunidadeId.HasValue && exportar is "excel" or "pdf")
            return exportar == "excel"
                ? File(_arquivos.AcoesExcel(model), TipoExcel, NomeArquivo("acoes", model.ComunidadeNome, "xlsx"))
                : File(_arquivos.AcoesPdf(model), "application/pdf", NomeArquivo("acoes", model.ComunidadeNome, "pdf"));

        await PopulateActiveCommunitiesAsync(comunidadeId);
        ViewBag.AtorList = new SelectList(
            await _context.Atores.AsNoTracking()
                .Where(actor => actor.Ativo == "S"
                    && (!comunidadeId.HasValue || actor.Comunidades.Any(link => link.FkIdComunidade == comunidadeId.Value)))
                .OrderBy(actor => actor.Nome).ToListAsync(HttpContext.RequestAborted),
            "IdAtores", "Nome", atorId);
        ViewBag.EixoList = new SelectList(await EixoCatalogo.ListarDisponiveisAsync(_context), "IdEixo", "Nome", eixoId);
        return View(model);
    }

    // Nome do arquivo baixado: relatório + comunidade/ator + data, só com caracteres seguros.
    private static string NomeArquivo(string relatorio, string? complemento, string extensao)
    {
        var limpo = new string((complemento ?? string.Empty).Normalize(System.Text.NormalizationForm.FormD)
            .Where(c => char.IsAsciiLetterOrDigit(c) || c is ' ' or '-')
            .ToArray()).Trim().Replace(' ', '-').ToLowerInvariant();
        return $"{relatorio}{(limpo.Length > 0 ? "-" + limpo : "")}-{DateTime.Now:yyyyMMdd}.{extensao}";
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

    // comunidadeId filtra a lista de atores (facilita achar o ator nas comunidades grandes).
    private async Task PopulateActiveActorsAsync(int? selectedId = null, int? comunidadeId = null)
    {
        ViewBag.AtorList = new SelectList(
            await _context.Atores.AsNoTracking()
                .Where(a => a.Ativo == "S"
                    && (comunidadeId == null || a.Comunidades.Any(link => link.FkIdComunidade == comunidadeId)))
                .OrderBy(a => a.Nome).ToListAsync(HttpContext.RequestAborted),
            "IdAtores", "Nome", selectedId);
        ViewBag.SelectedAtorId = selectedId;
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
