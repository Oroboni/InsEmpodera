using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Empodera.Controllers;

public class AtoresController : Controller
{
    private readonly ILogger<AtoresController> _logger;
    private readonly ApplicationDbContext _context;

    public AtoresController(ILogger<AtoresController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Resumo(Guid id)
    {
        if (!Guid.TryParse(HttpContext.Session.GetString("ID"), out var usuarioId))
            return RedirectToAction("Index", "Account");

        var usuario = await _context.Usuarios
            .Include(u => u.Perfil).ThenInclude(p => p.Permissoes)
            .FirstOrDefaultAsync(u => u.IdUsuario == usuarioId && u.Ativo == "S");
        if (usuario is null || !usuario.CanViewDetails("Atores"))
            return StatusCode(StatusCodes.Status403Forbidden);

        var ator = await _context.Atores.AsNoTracking()
            .Include(a => a.Comunidades).ThenInclude(c => c.Comunidade)
            .FirstOrDefaultAsync(a => a.IdAtores == id && a.Ativo != "N");
        return ator is null ? NotFound() : View(ator);
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == Guid.Parse(HttpContext.Session.GetString("ID") ?? Guid.Empty.ToString()) && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanList("Atores"))
        {
            return StatusCode(StatusCodes.Status403Forbidden);
        }

        ViewBag.qgenero = _context.Atores.Select(a => a.Genero).Distinct().ToList();

        var Atores = _context.Atores.Where(a => a.Ativo != "N").ToList();
        return View(Atores);
    }

    // GET: /Actor/Create
    [HttpGet]
    public async Task<IActionResult> Create(string? retorno = null, Guid? comunidadeId = null)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == Guid.Parse(HttpContext.Session.GetString("ID") ?? Guid.Empty.ToString()) && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanCreate("Atores"))
        {
            return RedirectToAction("Index", "Atores");
        }

        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.ToListAsync(),
            "Id_Comunidade",
            "Nome",
            comunidadeId
        );
        // "janela": cadastro aberto a partir do modal de redes; ao salvar, devolve o ator para a tela de origem.
        ViewBag.Retorno = retorno == RetornoJanela ? RetornoJanela : null;

        var novoAtor = new Atores
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now
        };

        await CarregarDadosRedesAsync(null, null);
        return View(novoAtor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Atores ator, Guid ComunidadeId, List<string>? recursos = null, List<string>? vulnerabilidades = null, int? tipoRelacionamento = null, List<RedeVinculoInput>? vinculosRede = null, bool redesEnviadas = false, string? retorno = null)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        ViewBag.Retorno = retorno == RetornoJanela ? RetornoJanela : null;

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == Guid.Parse(HttpContext.Session.GetString("ID") ?? Guid.Empty.ToString()) && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanCreate("Atores"))
        {
            return RedirectToAction("Index", "Atores");
        }

        var comunidadeExiste = await _context.Comunidades
            .AnyAsync(item => item.Id_Comunidade == ComunidadeId && item.Ativo != "N");
        ModelState.Remove(nameof(Atores.Usuario));
        if (!ModelState.IsValid || !comunidadeExiste)
        {
            if (!comunidadeExiste)
                ModelState.AddModelError(nameof(ComunidadeId), "A comunidade selecionada não existe ou está inativa.");
            ViewBag.Comunidades = new SelectList(
                await _context.Comunidades.Where(item => item.Ativo != "N").OrderBy(item => item.Nome).ToListAsync(),
                "Id_Comunidade", "Nome", ComunidadeId);
            await CarregarDadosRedesAsync(null, vinculosRede);
            return View(ator);
        }
        ator.IdAtores = Guid.NewGuid();
        ator.DtCriacao = DateTime.Now;
        ator.DtModificacao = DateTime.Now;
        ator.FkIdUsuario = Guid.Parse(HttpContext.Session.GetString("ID") ?? Guid.Empty.ToString());

        ator.ConfigureCreationAggregate(ComunidadeId, recursos, vulnerabilidades, tipoRelacionamento);
        _context.Atores.Add(ator);
        if (redesEnviadas)
            await SincronizarRedesAsync(ator.IdAtores, vinculosRede, ator.FkIdUsuario);
        await _context.SaveChangesAsync();

        if (ViewBag.Retorno == RetornoJanela)
        {
            ViewBag.ComunidadeIdCriado = ComunidadeId;
            return View("AtorCriado", ator);
        }

        return RedirectToAction("Index", "Atores");
    }


    // GET: /Actor/Edit/5
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (id == null)
            return NotFound();

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == Guid.Parse(HttpContext.Session.GetString("ID") ?? Guid.Empty.ToString()) && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanUpdate("Atores"))
        {
            return RedirectToAction("Index", "Atores");
        }

        var ator = await _context.Atores.FindAsync(id);
        if (ator == null)
            return NotFound();

        ViewBag.UsuarioOriginal = _context.Usuarios
            .Where(z => z.IdUsuario == ator.FkIdUsuario)
            .FirstOrDefault();
        ViewBag.UsuarioNovo = _context.Usuarios
            .Where(z => z.IdUsuario == ator.FkIdUsuarioM)
            .FirstOrDefault();

        var atorCom = await _context.AtorComunidades
            .FirstOrDefaultAsync(ac => ac.FK_id_Atores == id);
        ViewBag.TipoRelacionamento = atorCom?.TipoRelacionamento;

        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(),
            "Id_Comunidade",
            "Nome",
            atorCom?.FkIdComunidade 
        );

        await CarregarDadosRedesAsync(ator.IdAtores, null);
        return View(ator);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Atores atorFormulario, Guid id, Guid ComunidadeId, int? tipoRelacionamento = null, List<RedeVinculoInput>? vinculosRede = null, bool redesEnviadas = false)
    {
        if (HttpContext.Session.GetString("ID") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == Guid.Parse(HttpContext.Session.GetString("ID") ?? Guid.Empty.ToString()) && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanUpdate("Atores"))
        {
            return RedirectToAction("Index", "Atores");
        }

        if (!await _context.Comunidades.AnyAsync(item => item.Id_Comunidade == ComunidadeId && item.Ativo != "N"))
            return NotFound();
        var atorDb = await _context.Atores.FindAsync(id);
        if (atorDb == null)
        {
            return NotFound();
        }

        atorDb.Nome = atorFormulario.Nome;
        atorDb.Genero = atorFormulario.Genero;
        atorDb.Idade = atorFormulario.Idade;
        atorDb.PapelSocial1 = atorFormulario.PapelSocial1;
        atorDb.PapelSocial2 = atorFormulario.PapelSocial2;
        atorDb.Interesse = atorFormulario.Interesse;
        atorDb.Telefone = atorFormulario.Telefone;
        atorDb.DaEquipe = atorFormulario.DaEquipe;
        atorDb.Lopiniao = atorFormulario.Lopiniao;
        atorDb.Mcomunidade = atorFormulario.Mcomunidade;
        atorDb.Rope = atorFormulario.Rope;
        atorDb.FkIdUsuarioM = Guid.Parse(HttpContext.Session.GetString("ID") ?? Guid.Empty.ToString());
        atorDb.DtModificacao = DateTime.Now;

        var atorCom = await _context.AtorComunidades.FirstOrDefaultAsync(ac => ac.FK_id_Atores == id);

        if (atorCom == null)
        {
            atorCom = new AtorComunidade
            {
                FK_id_Atores = id,
                FkIdComunidade = ComunidadeId,
                TipoRelacionamento = tipoRelacionamento
            };
            _context.AtorComunidades.Add(atorCom);
        }
        else
        {
            atorCom.FkIdComunidade = ComunidadeId;
            // O campo saiu da tela (substituído pelas redes); preserva o valor já gravado.
            if (tipoRelacionamento.HasValue)
                atorCom.TipoRelacionamento = tipoRelacionamento;
        }

        if (redesEnviadas)
            await SincronizarRedesAsync(id, vinculosRede, atorDb.FkIdUsuarioM ?? atorDb.FkIdUsuario);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Edit), new { id });
    }


    // POST: /Actor/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(Guid? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == Guid.Parse(HttpContext.Session.GetString("ID") ?? Guid.Empty.ToString()) && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanDelete("Atores"))
        {
            return RedirectToAction("Index", "Atores");
        }

        if (id == null)
        {
            return NotFound();
        }

        var ator = await _context.Atores.FindAsync(id);
        if (ator != null)
        {
            ator.Ativo = "N";
            _context.Atores.Update(ator);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", "Atores");
        }
        return RedirectToAction("Index", "Atores");
    }

    private const string RetornoJanela = "janela";

    // Dados usados pelos modais de Rede Primária e Rede Subjetiva.
    private async Task CarregarDadosRedesAsync(Guid? atorId, List<RedeVinculoInput>? redesPostadas)
    {
        ViewBag.AtoresDisponiveisRede = await _context.AtorComunidades.AsNoTracking()
            .Where(ac => ac.Ator.Ativo != "N" && ac.FK_id_Atores != atorId)
            .OrderBy(ac => ac.Ator.Nome)
            .Select(ac => new { id = ac.FK_id_Atores, nome = ac.Ator.Nome, comunidadeId = ac.FkIdComunidade })
            .ToListAsync();

        ViewBag.ComunidadesRede = await _context.Comunidades.AsNoTracking()
            .Where(c => c.Ativo != "N")
            .OrderBy(c => c.Nome)
            .Select(c => new { id = c.Id_Comunidade, nome = c.Nome })
            .ToListAsync();

        if (redesPostadas is not null)
        {
            ViewBag.RedesAtuais = redesPostadas
                .Select(r => new { tipoRede = r.TipoRede, atorId = r.AtorId, nome = r.Nome, comunidadeId = r.ComunidadeId, tipoRelacao = r.TipoRelacao })
                .ToList();
            return;
        }

        ViewBag.RedesAtuais = atorId is null
            ? []
            : await _context.RedesPrimarias.AsNoTracking()
                .Where(r => r.FkIdAtorPrincipal == atorId && r.AtorRelacionado.Ativo != "N")
                .OrderBy(r => r.AtorRelacionado.Nome)
                .Select(r => new
                {
                    tipoRede = r.TipoRede,
                    atorId = (Guid?)r.FkIdAtorRelacionados,
                    nome = r.AtorRelacionado.Nome,
                    comunidadeId = (Guid?)null,
                    tipoRelacao = r.TipoRelacao
                })
                .ToListAsync<object>();
    }

    // Substitui os vínculos de rede do ator pelos enviados no formulário.
    // Ator relacionado sem Id é procurado pelo nome na comunidade informada; se não existir, é criado nela.
    private async Task SincronizarRedesAsync(Guid atorId, List<RedeVinculoInput>? redes, Guid usuarioId)
    {
        var existentes = await _context.RedesPrimarias.Where(r => r.FkIdAtorPrincipal == atorId).ToListAsync();
        _context.RedesPrimarias.RemoveRange(existentes);

        var novosAtores = new Dictionary<(Guid, string), Atores>();
        var vinculados = new HashSet<(string, Guid)>();

        foreach (var rede in redes ?? [])
        {
            var tipoRelacao = rede.TipoRelacao?.Trim() ?? string.Empty;
            if (!TiposRede.EhValido(rede.TipoRede) || tipoRelacao.Length is 0 or > 100)
                continue;

            Guid? relacionadoId = null;
            if (rede.AtorId is Guid idInformado)
            {
                if (await _context.Atores.AnyAsync(a => a.IdAtores == idInformado && a.Ativo != "N"))
                    relacionadoId = idInformado;
            }
            else if (!string.IsNullOrWhiteSpace(rede.Nome) && rede.ComunidadeId is Guid comunidadeId
                && await _context.Comunidades.AnyAsync(c => c.Id_Comunidade == comunidadeId && c.Ativo != "N"))
            {
                var nome = rede.Nome.Trim();
                if (nome.Length > 200)
                    continue;
                var nomeNormalizado = nome.ToLower();

                relacionadoId = await _context.AtorComunidades
                    .Where(ac => ac.FkIdComunidade == comunidadeId && ac.Ator.Ativo != "N" && ac.Ator.Nome.ToLower() == nomeNormalizado)
                    .Select(ac => (Guid?)ac.FK_id_Atores)
                    .FirstOrDefaultAsync();

                if (relacionadoId is null)
                {
                    if (!novosAtores.TryGetValue((comunidadeId, nomeNormalizado), out var novo))
                    {
                        novo = new Atores
                        {
                            IdAtores = Guid.NewGuid(),
                            Nome = nome,
                            DtCriacao = DateTime.Now,
                            DtModificacao = DateTime.Now,
                            FkIdUsuario = usuarioId
                        };
                        novo.ConfigureCreationAggregate(comunidadeId, null, null);
                        _context.Atores.Add(novo);
                        novosAtores[(comunidadeId, nomeNormalizado)] = novo;
                    }
                    relacionadoId = novo.IdAtores;
                }
            }

            if (relacionadoId is not Guid relacionado || relacionado == atorId || !vinculados.Add((rede.TipoRede, relacionado)))
                continue;

            _context.RedesPrimarias.Add(new RedePrimaria
            {
                IdRedePrimaria = Guid.NewGuid(),
                FkIdAtorPrincipal = atorId,
                FkIdAtorRelacionados = relacionado,
                TipoRede = rede.TipoRede,
                TipoRelacao = tipoRelacao
            });
        }
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
