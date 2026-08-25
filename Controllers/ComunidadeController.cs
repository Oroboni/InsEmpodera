using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering; 

namespace Empodera.Controllers;

public class ComunidadeController : Controller
{
    private readonly ILogger<ComunidadeController> _logger;
    private readonly ApplicationDbContext _context;

    public ComunidadeController(ILogger<ComunidadeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Comunidades")).FirstOrDefault();

        if (!PodeComunidade.CanList("Comunidades"))
        {
            return RedirectToAction("Index", "Home");
        }

        var comunidades = _context.Comunidades
            .Select(c => new Empodera.Models.ComunidadeDto
            {
                Id = c.Id_Comunidade,
                Nome = c.Nome,
                Status = c.Status,
                Ativo = c.Ativo
            })
            .Where(c => c.Ativo != "N")
            .ToList();

        return View(comunidades);
    }

    [HttpGet]
    public IActionResult ComunidadesDetalhes(int id)
    {
        if (HttpContext.Session.GetString("ID") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        Comunidade? comunidade;

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Comunidades")).FirstOrDefault();

        if (id > 0 && !PodeComunidade.CanUpdate("Comunidades"))
        {
            return RedirectToAction("Index", "Comunidade");
        }
        if (id <= 0 && !PodeComunidade.CanCreate("Comunidades"))
        {
            return RedirectToAction("Index", "Comunidade");
        }

        if (id > 0)
        {
            // Modo Edição: Busca a comunidade existente
            comunidade = _context.Comunidades.FirstOrDefault(c => c.Id_Comunidade == id);

            if (comunidade != null)
            {
                ViewBag.UsuarioOriginal = _context.Usuarios.Where(z => z.IdUsuario == comunidade.FK_Id_Usuario).FirstOrDefault();
                ViewBag.UsuarioNovo = _context.Usuarios.Where(z => z.IdUsuario == comunidade.FK_Id_UsuarioM).FirstOrDefault();
            }
            if (comunidade == null)
                return NotFound();
        }
        else
        {
            comunidade = new Comunidade();
            comunidade.Id_Comunidade = 0; 
        }

        var qAtores = _context.AtorComunidades.Include(a => a.Ator).Where(a => a.Ator.Ativo != "N").Count(a => a.FkIdComunidade == id);

        ViewBag.qAtores = qAtores;

        var qAtividades = _context.Atividades.Count(a => a.FkIdComunidade == id);
        ViewBag.qAtividades = qAtividades;

        var qRecursos = _context.RedeRecursos.Count(a => a.FkIdComunidade == id);
        ViewBag.qRecursos = qRecursos;

        return View(comunidade);
    }

[HttpPost]
[ValidateAntiForgeryToken]
public IActionResult ComunidadesDetalhes(Empodera.Models.Comunidade comunidade, int id)
{
    if (HttpContext.Session.GetString("Email") == null)
    {
        return RedirectToAction("Index", "Account");
    }
    
    var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
        .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Comunidades")).FirstOrDefault();

    // 1. Verifica se tem o módulo OU se o módulo nega criação/atualização.
    // Usuário SEM o módulo OU com permissões negadas (N) deve ser redirecionado.
    if ((comunidade.Id_Comunidade == 0 && !PodeComunidade.CanCreate("Comunidades")) || (comunidade.Id_Comunidade > 0 && !PodeComunidade.CanUpdate("Comunidades")))
    {
        return RedirectToAction("Index", "Comunidade");
    }

// Se a Id_Comunidade for 0, é uma nova criação
    if (comunidade.Id_Comunidade == 0)
    {
        comunidade.LocalMapa = BuildMapSearchAddress(comunidade.LocalMapa, comunidade.Local, comunidade.Nome);
        comunidade.Status = NormalizeCommunityStatus(comunidade.Status);

        if (string.IsNullOrWhiteSpace(comunidade.Local))
        {
            comunidade.Local = comunidade.LocalMapa;
        }

        comunidade.Dt_Criacao = DateTime.Now;
        comunidade.Dt_Modificacao = DateTime.Now;
        comunidade.FK_Id_Usuario = int.Parse(HttpContext.Session.GetString("ID") ?? "0");
        comunidade.LocalMapa ??= comunidade.Local;
        
        _context.Comunidades.Add(comunidade);
        _context.SaveChanges();
        
        return RedirectToAction("ComunidadesDetalhes", new { id = comunidade.Id_Comunidade });
    }
        
        // 2. Lógica de EDIÇÃO (Id_Comunidade > 0)
        var existingComunidade = _context.Comunidades.FirstOrDefault(c => c.Id_Comunidade == comunidade.Id_Comunidade);
        if (existingComunidade == null)
            return NotFound();

        {
            comunidade.LocalMapa = BuildMapSearchAddress(
                comunidade.LocalMapa,
                comunidade.Local,
                comunidade.Nome,
                existingComunidade.LocalMapa
            );
            comunidade.Status = NormalizeCommunityStatus(comunidade.Status);

            if (string.IsNullOrWhiteSpace(comunidade.Local))
            {
                comunidade.Local = comunidade.LocalMapa;
            }

            existingComunidade.Nome = comunidade.Nome;
            existingComunidade.Local = comunidade.Local;
            existingComunidade.LocalMapa = string.IsNullOrWhiteSpace(comunidade.LocalMapa)
                ? comunidade.Local
                : comunidade.LocalMapa;
            existingComunidade.Status = comunidade.Status;
            existingComunidade.Complemento = comunidade.Complemento;
            existingComunidade.Descricao = comunidade.Descricao;
            existingComunidade.Descricao_Acessibilidade = comunidade.Descricao_Acessibilidade;
            existingComunidade.Dt_Modificacao = DateTime.Now;
            existingComunidade.FK_Id_UsuarioM = int.Parse(HttpContext.Session.GetString("ID") ?? "0");

            _context.SaveChanges();
        }

        return RedirectToAction("ComunidadesDetalhes", new { id = comunidade.Id_Comunidade });

        // A
            //     var existingComunidade = _context.Comunidades.FirstOrDefault(c => c.Id_Comunidade == comunidade.Id_Comunidade);
            //     if (existingComunidade != null)
            //     {
            //         existingComunidade.Nome = comunidade.Nome;
            //         existingComunidade.Local = comunidade.Local;
            //         existingComunidade.Status = comunidade.Status;
            //         existingComunidade.Complemento = comunidade.Complemento;
            //         existingComunidade.Descricao = comunidade.Descricao;
            //         existingComunidade.DescricaoAcessibilidade = comunidade.DescricaoAcessibilidade;
            //         existingComunidade.DtModificacao = DateTime.Now;

            //         _context.SaveChanges();
            //     }

            // return RedirectToAction("Comunidades");
    }

    private static string BuildMapSearchAddress(
        string? explicitMapAddress,
        string? originalAddress,
        string? communityName,
        string? currentMapAddress = null)
    {
        var normalizedExplicit = NormalizeMapAddress(explicitMapAddress);
        var normalizedOriginal = NormalizeMapAddress(originalAddress);
        var normalizedCurrent = NormalizeMapAddress(currentMapAddress);
        var geographicAddress = ExtractGeographicAddress(normalizedOriginal);

        if (IsUsableMapAddress(normalizedExplicit, normalizedOriginal))
        {
            return normalizedExplicit;
        }

        if (!string.IsNullOrWhiteSpace(geographicAddress))
        {
            return geographicAddress;
        }

        if (IsUsableMapAddress(normalizedOriginal))
        {
            return normalizedOriginal;
        }

        if (IsUsableMapAddress(normalizedCurrent, normalizedOriginal))
        {
            return normalizedCurrent;
        }

        return NormalizeMapAddress(communityName);
    }

    private static string NormalizeMapAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim();
        normalized = normalized.Replace('\n', ' ');
        normalized = normalized.Replace('\r', ' ');

        while (normalized.Contains("  "))
        {
            normalized = normalized.Replace("  ", " ");
        }

        normalized = normalized.Replace(" - ", ", ");
        normalized = normalized.Replace(" / ", ", ");
        normalized = normalized.Replace(";", ",");

        return normalized.Trim(' ', ',', '-');
    }

    private static string ExtractGeographicAddress(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = NormalizeMapAddress(value);
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\([^)]*\)", " ");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\b[0-9A-Z]{4,}\+[0-9A-Z]{2,}\b", " ");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\b\d{4,6}\b", " ");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s{2,}", " ").Trim();

        var parts = normalized
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .ToList();

        if (parts.Count <= 1)
        {
            return normalized;
        }

        var filteredParts = parts
            .Where(part => !LooksLikeInstitutionName(part))
            .ToList();

        if (filteredParts.Count == 0)
        {
            return normalized;
        }

        if (filteredParts.Count < 2 && parts.Count >= 3)
        {
            return normalized;
        }

        return string.Join(", ", filteredParts);
    }

    private static bool IsUsableMapAddress(string? candidate, string? referenceOriginal = null)
    {
        if (string.IsNullOrWhiteSpace(candidate))
        {
            return false;
        }

        var parts = candidate
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 3)
        {
            return true;
        }

        if (candidate.Any(char.IsDigit) && parts.Length >= 2)
        {
            return true;
        }

        if (!string.IsNullOrWhiteSpace(referenceOriginal))
        {
            return candidate.Length >= Math.Max(18, referenceOriginal.Length / 2);
        }

        return candidate.Length >= 18 && parts.Length >= 2;
    }

    private static bool LooksLikeInstitutionName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        var institutionMarkers = new[]
        {
            "centro",
            "hospital",
            "healthcare",
            "fundacion",
            "fundação",
            "unidad",
            "penal",
            "rehabilit",
            "corrección",
            "correccion",
            "church",
            "escuela",
            "association",
            "asociación",
            "asociacion"
        };

        return institutionMarkers.Any(marker => value.Contains(marker, StringComparison.OrdinalIgnoreCase));
    }

    private static string NormalizeCommunityStatus(string? status)
    {
        if (string.IsNullOrWhiteSpace(status))
            return "Em diagnóstico";

        return status.Trim().ToLowerInvariant() switch
        {
            "em processo" => "Em processo",
            "em diagnóstico" or "em diagnostico" or "diagnosticado" => "Em diagnóstico",
            _ => status.Trim()
        };
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Comunidades")).FirstOrDefault();

        if (!PodeComunidade.CanDelete("Comunidades"))
        {
            return RedirectToAction("Index", "Comunidade");
        }

       var comunidade = await _context.Comunidades
            .Include(c => c.AtorComunidades)
            .ThenInclude(ac => ac.Ator).Where(c => c.Ativo != "N")
            .FirstOrDefaultAsync(c => c.Id_Comunidade == id);

        if (comunidade == null)
            return RedirectToAction("Index", "Comunidade");

        if (comunidade.AtorComunidades != null && comunidade.AtorComunidades.Any())
        {
            comunidade.AtorComunidades.ForEach(ac => ac.Ator.Ativo = "N");
        }

        comunidade.Ativo = "N";
        _context.Comunidades.Update(comunidade);
        await _context.SaveChangesAsync();

        return RedirectToAction("Index", "Comunidade");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Processo(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Comunidades")).FirstOrDefault();
        if (!PodeComunidade.CanUpdate("Comunidades"))
        {
            return RedirectToAction("Index", "Comunidade");
        }

        var comunidadebd = await _context.Comunidades
            .FirstOrDefaultAsync(c => c.Id_Comunidade == id);

        if (comunidadebd == null)
        {
            return NotFound();
        }

        if (comunidadebd.Status == "Em diagnóstico")
        {
            comunidadebd.Status = "Em processo";
            _context.Comunidades.Update(comunidadebd);
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("Index", "Comunidade");
    }

    public IActionResult AtoresVinculados(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanList("Atores"))
        {
            return RedirectToAction("Index", "Comunidade");
        }
        
        var AtorComunidades = _context.AtorComunidades
            .Include(ac => ac.Ator)
            .Where(ac => ac.FkIdComunidade == id && ac.Ator.Ativo != "N")
            .ToList();
        ViewData["id"] = id;

        var comunidade = _context.Comunidades.FirstOrDefault(c => c.Id_Comunidade == id);
        if (comunidade != null)
        {
            ViewBag.ComunidadeNome = comunidade.Nome;
        }

        ViewBag.ComunidadeId = id;

        return View(AtorComunidades);
    }

    //Recursos
    public async Task<IActionResult> ComunidadeRecursos(int comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Recursos")).FirstOrDefault();
        if (!PodeComunidade.CanList("Recursos"))
        {
            return RedirectToAction("ComunidadesDetalhes", "Comunidade");
        }

        var recursos = await _context.RedeRecursos
            .Include(r => r.Ator)
            .Include(r => r.Comunidade)       
            .Include(r => r.RedeEixos)  
                .ThenInclude(re => re.Eixo)
            .Where(r => r.FkIdComunidade == comunidadeId && r.Ator.Ativo != "N")
            .ToListAsync();

        ViewBag.ComunidadeId = comunidadeId;

        return View(recursos);
    }

    [HttpGet]
    public async Task<IActionResult> ComunidadeDetalhesRecursos(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (id == null || id == 0) return NotFound();

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Recursos")).FirstOrDefault();
        if (!PodeComunidade.CanViewDetails("Recursos"))
        {
            return RedirectToAction("Index", "Comunidade");
        }

        // 1. Busca na tabela RedeRecursos em vez de Atividades
        var recurso = await _context.RedeRecursos
            .Include(r => r.RedeEixos).ThenInclude(re => re.Eixo)
            .Include(r => r.Ator).Where(a => a.Ator.Ativo != "N")
            .FirstOrDefaultAsync(r => r.Id_Rede == id);

        if (recurso == null) return NotFound();

        // 2. Carrega listas para os Dropdowns (Atores e Comunidades)
        ViewBag.Comunidades = new SelectList(await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(), "Id_Comunidade", "Nome", recurso.FkIdComunidade);
        
        // Atores da comunidade para vincular o recurso
        var atores = await _context.AtorComunidades
            .Where(ac => ac.FkIdComunidade == recurso.FkIdComunidade)
            .Select(ac => ac.Ator).Where(a => a.Ativo != "N")
            .OrderBy(a => a.Nome)
            .ToListAsync();
        ViewBag.Atores = new SelectList(atores, "IdAtores", "Nome", recurso.FK_id_Atores);

        ViewBag.EixosList = await _context.Eixos.OrderBy(e => e.Nome).ToListAsync();
        
        // Informações de auditoria
       ViewBag.UsuarioOriginal = _context.Usuarios.FirstOrDefault(z => z.IdUsuario == recurso.FkIdUsuario);
        return View(recurso);
    }

    public async Task<IActionResult> Create_Recursos(int? comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null) return RedirectToAction("Index", "Account");

        if (comunidadeId == null || comunidadeId == 0) return NotFound();

        var PodeRecurso = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Recursos")).FirstOrDefault();
        if (!PodeRecurso.CanCreate("Recursos"))
        {
            return RedirectToAction("ComunidadeRecursos", "Comunidade");
        }

        ViewBag.ComunidadeId = comunidadeId;
        
        // Busca o nome da comunidade apenas para exibir na tela (opcional, mas bom para UX)
        var comunidade = await _context.Comunidades.FindAsync(comunidadeId);
        ViewBag.NomeComunidade = comunidade?.Nome;

        // Carrega Atores daquela comunidade para o Dropdown
        var atores = await _context.AtorComunidades
            .Where(ac => ac.FkIdComunidade == comunidadeId && ac.Ator.Ativo == "S")
            .Select(ac => ac.Ator).Where(a => a.Ativo != "N")
            .OrderBy(a => a.Nome)
            .ToListAsync();
        
        ViewBag.Atores = new SelectList(atores, "IdAtores", "Nome");
        
        // Carrega lista de Eixos
        ViewBag.EixosList = await _context.Eixos.OrderBy(e => e.Nome).ToListAsync();

        return View();
    }

    // POST: Recebe os dados e salva
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create_Recursos(RedeRecursos? recurso, List<int>? EixosSelecionados, int comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null) return RedirectToAction("Index", "Account");

        if (recurso == null)
        {
            return BadRequest();
        }
        if (EixosSelecionados == null)
        {
            return BadRequest();
        }

        var PodeRecurso = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Recursos")).FirstOrDefault();
        if (!PodeRecurso.CanCreate("Recursos"))
        {
            return RedirectToAction("ComunidadeRecursos", "Comunidade");
        }

        // Preenche dados automáticos
        recurso.FkIdComunidade = comunidadeId;
        recurso.DtCriacao = DateTime.Now;
        recurso.DtModificacao = DateTime.Now;
        recurso.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID") ?? "0");

        _context.RedeRecursos.Add(recurso);
        await _context.SaveChangesAsync();

        // Salva os Eixos
        if (EixosSelecionados != null)
        {
            foreach (var eixoId in EixosSelecionados)
            {
                _context.RedeEixos.Add(new RedeEixo { FkIdRede = recurso.Id_Rede, FkIdEixo = eixoId });
            }
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("ComunidadeRecursos", new { comunidadeId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit_Recursos(int id, RedeRecursos? recurso, List<int> EixosSelecionados)
    {
        if (HttpContext.Session.GetString("Email") == null) return RedirectToAction("Index", "Account");

        if (recurso == null)
        {
            return BadRequest();
        }

        var PodeRecurso = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Recursos")).FirstOrDefault();
        if (!PodeRecurso.CanUpdate("Recursos"))
        {
            return RedirectToAction("ComunidadeRecursos", "Comunidade");
        }

        var recursoDb = await _context.RedeRecursos
            .Include(r => r.RedeEixos)
            .FirstOrDefaultAsync(r => r.Id_Rede == id);

        if (recursoDb == null) return NotFound();

        recursoDb.Tipo = recurso.Tipo;
        recursoDb.Dispositivo = recurso.Dispositivo;
        recursoDb.Servicos = recurso.Servicos;
        recursoDb.FK_id_Atores = recurso.FK_id_Atores;
        recursoDb.DtModificacao = DateTime.Now;
        
        _context.RedeEixos.RemoveRange(recursoDb.RedeEixos);
        if (EixosSelecionados != null)
        {
            foreach (var eixoId in EixosSelecionados)
            {
                _context.RedeEixos.Add(new RedeEixo { FkIdRede = id, FkIdEixo = eixoId });
            }
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("ComunidadeRecursos", new { comunidadeId = recursoDb.FkIdComunidade });
    }

    // GET: /Actor/Create
    [HttpGet]
    public async Task<IActionResult> Create_Atores(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        if (id == null || id == 0)
        {
            return NotFound();
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanCreate("Atores"))
        {
            return RedirectToAction("AtoresVinculados", "Comunidade");
        }
        
        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.Where(c => c.Id_Comunidade == id).ToListAsync(), 
            "Id_Comunidade", 
            "Nome"
        );
        
        var novoAtor = new Atores
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now 
        };

        ViewBag.ComunidadeId = id;
        
        return View(novoAtor);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create_Atores(Atores ator, List<string>? recursos, List<string>? vulnerabilidades, int ComunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanCreate("Atores"))
        {
            return RedirectToAction("AtoresVinculados", "Comunidade");
        }

        var comunidadeExiste = await _context.Comunidades
            .AnyAsync(item => item.Id_Comunidade == ComunidadeId && item.Ativo != "N");
        ModelState.Remove(nameof(Atores.Usuario));
        if (!ModelState.IsValid || !comunidadeExiste)
        {
            if (!comunidadeExiste)
                ModelState.AddModelError(nameof(ComunidadeId), "A comunidade selecionada não existe ou está inativa.");

            ViewBag.Comunidades = new SelectList(
                await _context.Comunidades
                    .Where(item => item.Id_Comunidade == ComunidadeId && item.Ativo != "N")
                    .ToListAsync(),
                "Id_Comunidade",
                "Nome",
                ComunidadeId);
            ViewBag.ComunidadeId = ComunidadeId;
            return View(ator);
        }

        ator.DtCriacao = DateTime.Now;
        ator.DtModificacao = DateTime.Now;
        ator.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID") ?? "0");
        ator.ConfigureCreationAggregate(ComunidadeId, recursos, vulnerabilidades);

        _context.Atores.Add(ator);
        await _context.SaveChangesAsync();



        return RedirectToAction("AtoresVinculados", "Comunidade", new { id = ComunidadeId });
    }

    // GET: /Actor/Edit/5
    [HttpGet]
    public async Task<IActionResult> Edit_Atores(int id, int comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanUpdate("Atores"))
        {
            return RedirectToAction("AtoresVinculados", "Comunidade");
        }

        var ator = await _context.Atores.Include(c => c.RecursosAtores).Where(a => a.IdAtores == id).FirstOrDefaultAsync();
        if (ator == null)
        {
            return NotFound();
        }

        ViewBag.UsuarioOriginal = _context.Usuarios.Where(z => z.IdUsuario == ator.FkIdUsuario).FirstOrDefault();
        ViewBag.UsuarioNovo = _context.Usuarios.Where(z => z.IdUsuario == ator.FkIdUsuarioM).FirstOrDefault();

        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.Where(c => c.Id_Comunidade == comunidadeId).ToListAsync(),
            "Id_Comunidade",
            "Nome"
        );

        ViewBag.ComunidadeId = comunidadeId;

        return View(ator);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit_Atores(Atores ator, int ComunidadeId, List<string>? recursos, List<string>? vulnerabilidades)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanUpdate("Atores"))
        {
            return RedirectToAction("AtoresVinculados", "Comunidade");
        }

        var atorDb = await _context.Atores.FindAsync(ator.IdAtores);
        if (atorDb == null)
            return NotFound();

        atorDb.Nome = ator.Nome;
        atorDb.Genero = ator.Genero;
        atorDb.Idade = ator.Idade;
        atorDb.PapelSocial1 = ator.PapelSocial1;
        atorDb.PapelSocial2 = ator.PapelSocial2;
        atorDb.Telefone = ator.Telefone;
        atorDb.DaEquipe = ator.DaEquipe;
        atorDb.Lopiniao = ator.Lopiniao;
        atorDb.Mcomunidade = ator.Mcomunidade;
        atorDb.Rope = ator.Rope;
        atorDb.FkIdUsuarioM = int.Parse(HttpContext.Session.GetString("ID") ?? "0");
        atorDb.DtModificacao = DateTime.Now;

        var recursosAtores = await _context.RecursosAtores
            .Where(r => r.FK_id_Atores == ator.IdAtores)
            .ToListAsync();

        foreach (var recursoAtor in recursosAtores)
        {
            if (recursoAtor.Tipo == "Recurso")
            {
                recursoAtor.Pode = recursos?.Contains(recursoAtor.Nome) == true ? "S" : "N";
            }
            else if (recursoAtor.Tipo == "Vulnerabilidade")
            {
                recursoAtor.Pode = vulnerabilidades?.Contains(recursoAtor.Nome) == true ? "S" : "N";
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("AtoresVinculados", "Comunidade", new { id = ComunidadeId });
    }


    // GET: /Actor/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete_Atores(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        if (id == null)
        {
            return NotFound();
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (!PodeComunidade.CanDelete("Atores"))
        {
            return RedirectToAction("AtoresVinculados", "Comunidade");
        }
        
        var ator = await _context.Atores.FindAsync(id);
        if (ator != null)
        {
            ator.Ativo = "N";
            _context.Atores.Update(ator);
            await _context.SaveChangesAsync();
        }
        var atorCom = await _context.AtorComunidades
            .FirstOrDefaultAsync(ac => ac.FK_id_Atores == id);
            
        return RedirectToAction("AtoresVinculados", "Comunidade", new { id = atorCom?.FkIdComunidade });
    }

    public async Task<IActionResult> AtividadesVinculadas(int comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodeAtividades = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atividades")).FirstOrDefault();
        if (!PodeAtividades.CanList("Atividades"))
        {
            return RedirectToAction("Index", "Comunidade");
        }

        var atividades = await _context.Atividades
            .Include(a => a.AtividadesEixos)
            .ThenInclude(ae => ae.Eixo)
            .Where(a => a.FkIdComunidade == comunidadeId)
            .ToListAsync();

        ViewBag.ComunidadeId = comunidadeId;

        return View(atividades);
    }

    public async Task<IActionResult> Create_Atividades(int comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodeAtividades = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atividades")).FirstOrDefault();
        if (!PodeAtividades.CanCreate("Atividades"))
        {
            return RedirectToAction("AtividadesVinculadas", "Comunidade");
        }

        ViewBag.EixosList = await _context.Eixos.OrderBy(e => e.Nome).ToListAsync();

        ViewBag.comunidadeId = comunidadeId;

        var model = new Atividades
        {
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create_Atividades(Atividades atividade, List<int> EixosSelecionados, int comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodeAtividades = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atividades")).FirstOrDefault();
        if (!PodeAtividades.CanCreate("Atividades"))
        {
            return RedirectToAction("Index", "Comunidade");
        }

        Console.WriteLine("é o " + comunidadeId);
        atividade.DtCriacao = DateTime.Now;
        atividade.DtModificacao = DateTime.Now;
        atividade.FkIdComunidade = comunidadeId;
        atividade.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID") ?? "0");

        _context.Atividades.Add(atividade);
        await _context.SaveChangesAsync();

        if (EixosSelecionados != null && EixosSelecionados.Count > 0)
        {
            foreach (var eixoId in EixosSelecionados)
            {
                _context.AtividadesEixo.Add(new AtividadesEixo
                {
                    FkIdAtividade = atividade.IdAtividade,
                    FkIdEixo = eixoId
                });
            }
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("AtividadesVinculadas", new { comunidadeId });
    }

    public async Task<IActionResult> Edit_Atividades(int? id, int comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (id == null) return NotFound();

        var PodeAtividades = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atividades")).FirstOrDefault();
        if (!PodeAtividades.CanUpdate("Atividades"))
        {
            return RedirectToAction("AtividadesVinculadas", "Comunidade");
        }

        var atividade = await _context.Atividades
            .Include(a => a.AtividadesEixos)
            .ThenInclude(ae => ae.Eixo)
            .FirstOrDefaultAsync(a => a.IdAtividade == id && a.FkIdComunidade == comunidadeId);

        if (atividade == null) return NotFound();

        ViewBag.comunidadeId = comunidadeId;
        ViewBag.UsuarioOriginal = _context.Usuarios.Where(z => z.IdUsuario == atividade.FkIdUsuario).FirstOrDefault();
        ViewBag.UsuarioNovo = _context.Usuarios.Where(z => z.IdUsuario == atividade.FkIdUsuarioM).FirstOrDefault();

        ViewBag.EixosList = await _context.Eixos.OrderBy(e => e.Nome).ToListAsync();

        return View(atividade);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit_Atividades(int id, Atividades atividade, List<int> EixosSelecionados, int comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (id != atividade.IdAtividade) return NotFound();

        var PodeAtividades = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atividades")).FirstOrDefault();
        if (!PodeAtividades.CanUpdate("Atividades"))
        {
            return RedirectToAction("AtividadesVinculadas", "Comunidade");
        }

        var existingAtividade = await _context.Atividades
            .Include(a => a.AtividadesEixos)
            .FirstOrDefaultAsync(a => a.IdAtividade == id && a.FkIdComunidade == comunidadeId);

        if (existingAtividade == null) return NotFound();

        existingAtividade.Nome = atividade.Nome;
        existingAtividade.Descricao = atividade.Descricao;
        existingAtividade.DtModificacao = DateTime.Now;
        existingAtividade.FkIdUsuarioM = int.Parse(HttpContext.Session.GetString("ID") ?? "0");

        var existingEixoIds = existingAtividade.AtividadesEixos.Select(ae => ae.FkIdEixo).ToList();

        var eixosToAdd = EixosSelecionados.Except(existingEixoIds).ToList();
        var eixosToRemove = existingEixoIds.Except(EixosSelecionados).ToList();

        foreach (var eixoId in eixosToAdd)
        {
            _context.AtividadesEixo.Add(new AtividadesEixo
            {
                FkIdAtividade = existingAtividade.IdAtividade,
                FkIdEixo = eixoId
            });
        }

        foreach (var eixoId in eixosToRemove)
        {
            var atividadeEixo = await _context.AtividadesEixo
                .FirstOrDefaultAsync(ae => ae.FkIdAtividade == existingAtividade.IdAtividade && ae.FkIdEixo == eixoId);
            if (atividadeEixo != null)
            {
                _context.AtividadesEixo.Remove(atividadeEixo);
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction("AtividadesVinculadas", new { comunidadeId });

    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
