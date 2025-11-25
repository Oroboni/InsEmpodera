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
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }
        Comunidade comunidade;

        if (id > 0)
        {
            // Modo Edição: Busca a comunidade existente
            comunidade = _context.Comunidades.Where(a => a.Ativo == "S").FirstOrDefault(c => c.IdComunidade == id);

            ViewBag.UsuarioOriginal = _context.Usuarios.Where(z => z.IdUsuario == comunidade.FkIdUsuario).FirstOrDefault();
            ViewBag.UsuarioNovo = _context.Usuarios.Where(z => z.IdUsuario == comunidade.FkIdUsuarioM).FirstOrDefault();
            
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

        var qAtores = _context.AtorComunidades.Count(a => a.FkIdComunidade == id);

        ViewBag.qAtores = qAtores;

        var qAtividades = _context.Atividades.Count(a => a.FkIdComunidade == id);
        ViewBag.qAtividades = qAtividades;

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
        // 1. Lógica de CRIAÇÃO (IdComunidade == 0)
        if (comunidade.IdComunidade == 0)
        {
            comunidade.DtCriacao = DateTime.Now;
            comunidade.DtModificacao = DateTime.Now;
            comunidade.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID"));
            
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
            existingComunidade.FkIdUsuarioM = int.Parse(HttpContext.Session.GetString("ID"));

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
        if (HttpContext.Session.GetString("Email") == null)
            return RedirectToAction("Index", "Account");

        if (id == null)
            return NotFound();

       var comunidade = await _context.Comunidades
            .Include(c => c.AtorComunidades)
            .ThenInclude(ac => ac.Ator)
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


    public async Task<IActionResult> Processo(int id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
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

    public IActionResult AtoresVinculados(int id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
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

    // GET: /Actor/Create
    [HttpGet]
    public async Task<IActionResult> Create_Atores(int id)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
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

        ator.DtCriacao = DateTime.Now;
        ator.DtModificacao = DateTime.Now;
        ator.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID"));

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
        atorDb.FkIdUsuarioM = int.Parse(HttpContext.Session.GetString("ID"));
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

        Console.WriteLine("é o " + comunidadeId);
        atividade.DtCriacao = DateTime.Now;
        atividade.DtModificacao = DateTime.Now;
        atividade.FkIdComunidade = comunidadeId;
        atividade.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID"));

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

        var existingAtividade = await _context.Atividades
            .Include(a => a.AtividadesEixos)
            .FirstOrDefaultAsync(a => a.IdAtividade == id && a.FkIdComunidade == comunidadeId);

        if (existingAtividade == null) return NotFound();

        existingAtividade.Nome = atividade.Nome;
        existingAtividade.Descricao = atividade.Descricao;
        existingAtividade.DtModificacao = DateTime.Now;
        existingAtividade.FkIdUsuarioM = int.Parse(HttpContext.Session.GetString("ID"));

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