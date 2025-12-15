using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using SQLitePCL;
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

        if (PodeComunidade == null || PodeComunidade.Perfil.Permissoes.Any(p => p.PodeListar == "N"))
        {
            return RedirectToAction("Index", "Home");
        }

        var comunidades = _context.Comunidades
            .Select(c => new Empodera.Models.ComunidadeDto
            {
                Id = c.IdComunidade,
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

        if (PodeComunidade == null || PodeComunidade.Perfil.Permissoes.Any(p => p.PodeDetalhar == "N"))
        {
            return RedirectToAction("Index", "Comunidade");
        }
        if (PodeComunidade.Perfil.Permissoes.Any(p => p.PodeCriar == "N") || PodeComunidade.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
        {
            return RedirectToAction("Index", "Comunidade");
        }

        if (id > 0)
        {
            // Modo Edição: Busca a comunidade existente
            comunidade = _context.Comunidades.FirstOrDefault(c => c.IdComunidade == id);

            if (comunidade != null)
            {
                ViewBag.UsuarioOriginal = _context.Usuarios.Where(z => z.IdUsuario == comunidade.FkIdUsuario).FirstOrDefault();
                ViewBag.UsuarioNovo = _context.Usuarios.Where(z => z.IdUsuario == comunidade.FkIdUsuarioM).FirstOrDefault();
            }
            
            // Se não encontrar, retorna um modelo vazio para o modo de criação/ou erro, 
            // dependendo da sua regra de negócio. Para simplificar, trataremos como novo.
            if (comunidade == null)
            {
                comunidade = new Comunidade();
            }
        }
        else
        {
            comunidade = new Comunidade();
            comunidade.IdComunidade = 0; 
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
    if (PodeComunidade == null || (comunidade.IdComunidade == 0 && PodeComunidade.Perfil.Permissoes.Any(p => p.PodeCriar == "N")) || (comunidade.IdComunidade > 0 && PodeComunidade.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N")))
    {
        return RedirectToAction("Index", "Comunidade");
    }
    
    // OBS: O código original tinha duas verificações. Simplificando para a verificação correta do bloco original:
    /*
    if (PodeComunidade == null)
    {
         return RedirectToAction("Index", "Comunidade");
    }
    if (PodeComunidade.Perfil.Permissoes.Any(p => p.PodeCriar == "N") && comunidade.IdComunidade == 0)
    {
        return RedirectToAction("Index", "Comunidade");
    }
    if (PodeComunidade.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N") && comunidade.IdComunidade > 0)
    {
        return RedirectToAction("Index", "Comunidade");
    }
    */

    // Se a IdComunidade for 0, é uma nova criação
    if (comunidade.IdComunidade == 0)
    {
        comunidade.DtCriacao = DateTime.Now;
        comunidade.DtModificacao = DateTime.Now;
        comunidade.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID") ?? "0");
        
        _context.Comunidades.Add(comunidade);
        _context.SaveChanges();
        
        return RedirectToAction("ComunidadesDetalhes", new { id = comunidade.IdComunidade });
    }
        
        // 2. Lógica de EDIÇÃO (IdComunidade > 0)
        var existingComunidade = _context.Comunidades.FirstOrDefault(c => c.IdComunidade == comunidade.IdComunidade);
        if (existingComunidade != null)
        {
            existingComunidade.Nome = comunidade.Nome;
            existingComunidade.Local = comunidade.Local;
            existingComunidade.Status = comunidade.Status;
            existingComunidade.Complemento = comunidade.Complemento;
            existingComunidade.Descricao = comunidade.Descricao;
            existingComunidade.DescricaoAcessibilidade = comunidade.DescricaoAcessibilidade;
            existingComunidade.DtModificacao = DateTime.Now;
            existingComunidade.FkIdUsuarioM = int.Parse(HttpContext.Session.GetString("ID") ?? "0");

            _context.SaveChanges();
        }

        return RedirectToAction("ComunidadesDetalhes", new { id = comunidade.IdComunidade });

        // A
            //     var existingComunidade = _context.Comunidades.FirstOrDefault(c => c.IdComunidade == comunidade.IdComunidade);
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

        if (PodeComunidade == null ||PodeComunidade.Perfil.Permissoes.Any(p => p.PodeDeletar == "N"))
        {
            return RedirectToAction("Index", "Comunidade");
        }

       var comunidade = await _context.Comunidades
            .Include(c => c.AtorComunidades)
            .ThenInclude(ac => ac.Ator).Where(c => c.Ativo != "N")
            .FirstOrDefaultAsync(c => c.IdComunidade == id);

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
        if (PodeComunidade == null || PodeComunidade.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
        {
            return RedirectToAction("Index", "Comunidade");
        }

        var comunidadebd = await _context.Comunidades
            .FirstOrDefaultAsync(c => c.IdComunidade == id);

        if (comunidadebd == null)
        {
            return NotFound();
        }

        if (comunidadebd.Status == "Em diagnóstico")
        {
            comunidadebd.Status = "Em Processo";
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
        if (PodeComunidade == null || PodeComunidade.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
        {
            return RedirectToAction("Index", "Comunidade");
        }
        
        var AtorComunidades = _context.AtorComunidades
            .Include(ac => ac.Ator)
            .Where(ac => ac.FkIdComunidade == id && ac.Ator.Ativo != "N")
            .ToList();
        ViewData["id"] = id;

        var comunidade = _context.Comunidades.FirstOrDefault(c => c.IdComunidade == id);
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
        if (PodeComunidade == null || PodeComunidade.Perfil.Permissoes.Any(p => p.PodeDetalhar == "N"))
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
        if (PodeComunidade == null || PodeComunidade.Perfil.Permissoes.Any(p => p.PodeListar == "N"))
        {
            return RedirectToAction("Index", "Comunidade");
        }

        // 1. Busca na tabela RedeRecursos em vez de Atividades
        var recurso = await _context.RedeRecursos
            .Include(r => r.RedeEixos).ThenInclude(re => re.Eixo)
            .Include(r => r.Ator).Where(a => a.Ator.Ativo != "N")
            .FirstOrDefaultAsync(r => r.IdRede == id);

        if (recurso == null) return NotFound();

        // 2. Carrega listas para os Dropdowns (Atores e Comunidades)
        ViewBag.Comunidades = new SelectList(await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(), "IdComunidade", "Nome", recurso.FkIdComunidade);
        
        // Atores da comunidade para vincular o recurso
        var atores = await _context.AtorComunidades
            .Where(ac => ac.FkIdComunidade == recurso.FkIdComunidade)
            .Select(ac => ac.Ator).Where(a => a.Ativo != "N")
            .OrderBy(a => a.Nome)
            .ToListAsync();
        ViewBag.Atores = new SelectList(atores, "IdAtores", "Nome", recurso.FKidAtores);

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
        if (PodeRecurso == null || PodeRecurso.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
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
        if (PodeRecurso == null || PodeRecurso.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
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
                _context.RedeEixos.Add(new RedeEixo { FkIdRede = recurso.IdRede, FkIdEixo = eixoId });
            }
            await _context.SaveChangesAsync();
        }

        return RedirectToAction("ComunidadeRecursos", new { comunidadeId });
    }

    // Adicione também o POST para Salvar as edições dessa tela
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
        if (PodeRecurso == null || PodeRecurso.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
        {
            return RedirectToAction("ComunidadeRecursos", "Comunidade");
        }

        var recursoDb = await _context.RedeRecursos
            .Include(r => r.RedeEixos)
            .FirstOrDefaultAsync(r => r.IdRede == id);

        if (recursoDb == null) return NotFound();

        // Atualiza campos
        recursoDb.Tipo = recurso.Tipo;
        recursoDb.Dispositivo = recurso.Dispositivo;
        recursoDb.Servicos = recurso.Servicos;
        recursoDb.FKidAtores = recurso.FKidAtores;
        recursoDb.DtModificacao = DateTime.Now;
        
        // Atualiza Eixos
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
        if (PodeComunidade == null || PodeComunidade.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
        {
            return RedirectToAction("AtoresVinculados", "Comunidade");
        }
        
        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.Where(c => c.IdComunidade == id).ToListAsync(), 
            "IdComunidade", 
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
    public async Task<IActionResult> Create_Atores(Atores ator, int ComunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (PodeComunidade == null || PodeComunidade.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
        {
            return RedirectToAction("AtoresVinculados", "Comunidade");
        }

        ator.DtCriacao = DateTime.Now;
        ator.DtModificacao = DateTime.Now;
        ator.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID") ?? "0");

        _context.Atores.Add(ator);
        await _context.SaveChangesAsync();

        var relacao = new AtorComunidade
        {
            FkIdComunidade = ComunidadeId,
            FKidAtores = ator.IdAtores
        };

        _context.AtorComunidades.Add(relacao);
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
        if (PodeComunidade == null || PodeComunidade.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
        {
            return RedirectToAction("AtoresVinculados", "Comunidade");
        }

        var ator = await _context.Atores.FindAsync(id);
        if (ator == null)
        {
            return NotFound();
        }

        ViewBag.UsuarioOriginal = _context.Usuarios.Where(z => z.IdUsuario == ator.FkIdUsuario).FirstOrDefault();
        ViewBag.UsuarioNovo = _context.Usuarios.Where(z => z.IdUsuario == ator.FkIdUsuarioM).FirstOrDefault();

        ViewBag.Comunidades = new SelectList(
            await _context.Comunidades.Where(c => c.IdComunidade == comunidadeId).ToListAsync(),
            "IdComunidade",
            "Nome"
        );

        ViewBag.ComunidadeId = comunidadeId;

        return View(ator);
    }



    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit_Atores(Atores ator, int ComunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodeComunidade = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atores")).FirstOrDefault();
        if (PodeComunidade == null || PodeComunidade.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
        {
            return RedirectToAction("AtoresVinculados", "Comunidade");
        }

        var atorDb = await _context.Atores.FindAsync(ator.IdAtores);
        if (atorDb == null)
            return NotFound();

        atorDb.Nome = ator.Nome;
        atorDb.Genero = ator.Genero;
        atorDb.DtNascimento = ator.DtNascimento;
        atorDb.PapelSocial1 = ator.PapelSocial1;
        atorDb.PapelSocial2 = ator.PapelSocial2;
        atorDb.Telefone = ator.Telefone;
        atorDb.DaEquipe = ator.DaEquipe;
        atorDb.Lopiniao = ator.Lopiniao;
        atorDb.Mcomunidade = ator.Mcomunidade;
        atorDb.Rope = ator.Rope;
        atorDb.FkIdUsuarioM = int.Parse(HttpContext.Session.GetString("ID") ?? "0");
        atorDb.DtModificacao = DateTime.Now;

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
        if (PodeComunidade == null || PodeComunidade.Perfil.Permissoes.Any(p => p.PodeDeletar == "N"))
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
            .FirstOrDefaultAsync(ac => ac.FKidAtores == id);
            
        return RedirectToAction("AtoresVinculados", "Comunidade", new { id = atorCom?.FkIdComunidade });
    }

    public async Task<IActionResult> AtividadesVinculadas(int comunidadeId)
    {
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        var PodeAtividades = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Atividades")).FirstOrDefault();
        if (PodeAtividades == null || PodeAtividades.Perfil.Permissoes.Any(p => p.PodeListar == "N"))
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
        if (PodeAtividades == null || PodeAtividades.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
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
        if (PodeAtividades == null || PodeAtividades.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
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
        if (PodeAtividades == null || PodeAtividades.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
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
        if (PodeAtividades == null || PodeAtividades.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
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