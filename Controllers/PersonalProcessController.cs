using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Empodera.Controllers;

public class PersonalProcessController : Controller
{
    private readonly ApplicationDbContext _context;

    public PersonalProcessController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /PersonalProcess/Index
    public async Task<IActionResult> Index(int? atorId, string searchQuery)
    {
        if (HttpContext.Session.GetString("Email") == null)
        {
            return RedirectToAction("Index", "Account");
        }

        var PodeProcesso = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
        .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "AvaliacoesPessoais")).FirstOrDefault();
        if (PodeProcesso == null || PodeProcesso.Perfil.Permissoes.Any(p => p.PodeListar == "N"))
        {
            return RedirectToAction("Index", "PersonalProcess");
        }

        // 1. Carregar lista de Atores
        ViewBag.AtorList = new SelectList(
            await _context.Atores.OrderBy(a => a.Nome).ToListAsync(),
            "IdAtores",
            "Nome",
            atorId 
        );
        
        ViewBag.SelectedAtorId = atorId;
        ViewBag.SearchQuery = searchQuery;

        // 2. Prepara a consulta
        var query = _context.DiariosCampo.AsQueryable();

        // 3. Filtra por Ator (Agora usando o campo novo AtorId)
        if (atorId.HasValue)
        {
            // Filtra onde AtorId é igual ao selecionado
            query = query.Where(d => d.FkIdUsuario == atorId.Value);
        }
        else 
        {
            // Se não selecionou ator, retorna lista vazia (Estado inicial)
            return View(new List<DiarioCampo>());
        }

        // 4. Busca por texto (opcional)
        if (!string.IsNullOrEmpty(searchQuery))
        {
            query = query.Where(d => d.Descricao.Contains(searchQuery));
        }

        // 5. Ordena e Executa
        var diarios = await query.OrderByDescending(d => d.Data).ToListAsync();

        return View(diarios);
    }

    // GET: /PersonalProcess/Create
    public async Task<IActionResult> Create(int atorId)
    {
        if (HttpContext.Session.GetString("Email") == null) 
        { 
            return RedirectToAction("Index", "Account"); 
        }

        var PodeProcesso = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
        .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "AvaliacoesPessoais")).FirstOrDefault();
        if (PodeProcesso == null || PodeProcesso.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
        {
            return RedirectToAction("Index", "PersonalProcess");
        }
        
        ViewBag.AtorList = new SelectList(
            await _context.Atores.OrderBy(a => a.Nome).ToListAsync(), 
            "IdAtores", 
            "Nome", 
            atorId
        );
        
        ViewBag.EixosList = new SelectList(
            await _context.Eixos.OrderBy(e => e.Nome).ToListAsync(), 
            "IdEixo", 
            "Nome"
        );

        var model = new DiarioCampo 
        { 
            Data = DateTime.Now,
            DtCriacao = DateTime.Now,
            DtModificacao = DateTime.Now,
            FkIdUsuario = atorId 
        };

        return View(model);
    }

    // GET: /PersonalProcess/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (HttpContext.Session.GetString("Email") == null) 
        { 
            return RedirectToAction("Index", "Account"); 
        }

        var PodeProcesso = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
        .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "AvaliacoesPessoais")).FirstOrDefault();
        if (PodeProcesso == null || PodeProcesso.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
        {
            return RedirectToAction("Index", "PersonalProcess");
        }

        if (id == null) return NotFound();

        var diario = await _context.DiariosCampo.FindAsync(id);
        if (diario == null) return NotFound();
        
        ViewBag.AtorList = new SelectList(
            await _context.Atores.OrderBy(a => a.Nome).ToListAsync(), 
            "IdAtores", 
            "Nome", 
            diario.FkIdUsuario // Seleciona o ator salvo
        );
        
        ViewBag.EixosList = new SelectList(await _context.Eixos.OrderBy(e => e.Nome).ToListAsync(), "IdEixo", "Nome");

        return View(diario);
    }
    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(DiarioCampo diario, int SelectedAtorId, int[] eixosIds)
    {
        if (HttpContext.Session.GetString("Email") == null) 
        { 
            return RedirectToAction("Index", "Account"); 
        }

        var PodeProcesso = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
        .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "AvaliacoesPessoais")).FirstOrDefault();
        if (PodeProcesso == null || PodeProcesso.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
        {
            return RedirectToAction("Index", "PersonalProcess");
        }

        try 
        {
            // 1. Vincula o ID do Ator vindo do select (HTML) ao objeto Diario
            if (SelectedAtorId > 0)
            {
                diario.FkIdUsuario = SelectedAtorId;
            }
            else
            {
                // Se não selecionou ator, força erro para não salvar órfão
                ModelState.AddModelError("AtorId", "Selecione um ator.");
                throw new Exception("Ator obrigatório");
            }

            // 2. Preenche as datas automáticas
            diario.DtCriacao = DateTime.Now;
            diario.DtModificacao = DateTime.Now;

            // 3. Salva o Diário no Banco (Isso gera o ID do diário)
            _context.Add(diario);
            await _context.SaveChangesAsync();

            // 4. Salva os Eixos selecionados (Tags) na tabela de ligação
            if (eixosIds != null && eixosIds.Length > 0)
            {
                foreach (var eixoId in eixosIds)
                {
                    var vinculo = new DiarioEixo
                    {
                        FkIdDiario = diario.IdDCampo, // ID gerado acima
                        FkIdEixo = eixoId
                    };
                    _context.Add(vinculo);
                }
                // Salva os vínculos dos eixos
                await _context.SaveChangesAsync();
            }

            // 5. Redireciona para o Index (filtrando pelo ator que acabamos de criar)
            return RedirectToAction(nameof(Index), new { atorId = diario.FkIdUsuario });
        }
        catch (Exception)
        {
            // Se der erro, recarrega os dropdowns para a tela não quebrar
            ViewBag.AtorList = new SelectList(await _context.Atores.OrderBy(a => a.Nome).ToListAsync(), "IdAtores", "Nome", SelectedAtorId);
            ViewBag.EixosList = new SelectList(await _context.Eixos.OrderBy(e => e.Nome).ToListAsync(), "IdEixo", "Nome");
            
            return View(diario);
        }
    }
    
    // POST: /PersonalProcess/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, DiarioCampo diario)
    {

        if (HttpContext.Session.GetString("Email") == null) 
        { 
            return RedirectToAction("Index", "Account"); 
        }

        var PodeProcesso = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
        .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "AvaliacoesPessoais")).FirstOrDefault();
        if (PodeProcesso == null || PodeProcesso.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
        {
            return RedirectToAction("Index", "PersonalProcess");
        }
         if (id != diario.IdDCampo) return NotFound();

         if (ModelState.IsValid)
         {
             try
             {
                 var diarioExistente = await _context.DiariosCampo.FindAsync(id);
                 if(diarioExistente == null) return NotFound();

                 // Atualiza os campos
                 diarioExistente.FkIdUsuario = diario.FkIdUsuario;
                 diarioExistente.Data = diario.Data;
                 diarioExistente.Descricao = diario.Descricao;
                 diarioExistente.DtModificacao = DateTime.Now;

                 _context.Update(diarioExistente);
                 await _context.SaveChangesAsync();
             }
             catch (DbUpdateConcurrencyException)
             {
                 if (!_context.DiariosCampo.Any(e => e.IdDCampo == id)) return NotFound();
                 else throw;
             }
             return RedirectToAction(nameof(Index), new { atorId = diario.FkIdUsuario });
         }
         
         ViewBag.AtorList = new SelectList(await _context.Atores.OrderBy(a => a.Nome).ToListAsync(), "IdAtores", "Nome", diario.FkIdUsuario);
         ViewBag.EixosList = new SelectList(await _context.Eixos.OrderBy(e => e.Nome).ToListAsync(), "IdEixo", "Nome");
         
         return View(diario);
    }
}