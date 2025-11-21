using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using InsEmpodera.Models;
using Empodera.Data;
using Empodera.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace InsEmpodera.Controllers
{
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
            // Carrega lista para o filtro
            ViewBag.AtorList = new SelectList(await _context.Atores.OrderBy(a => a.Nome).ToListAsync(), "IdAtores", "Nome", atorId);
            ViewBag.SelectedAtorId = atorId; // Importante para manter o estado
            ViewBag.SearchQuery = searchQuery;

            // Se não selecionou ator, retorna lista vazia (ou view de "selecione")
            if (!atorId.HasValue) return View(new List<DiarioCampo>());

            var query = _context.DiariosCampo.Include(d => d.Ator).AsQueryable();

            if (atorId.HasValue) query = query.Where(d => d.AtorId == atorId);
            if (!string.IsNullOrEmpty(searchQuery)) query = query.Where(d => d.Descricao.Contains(searchQuery));

            var result = await query.OrderByDescending(d => d.Data).ToListAsync();

            // --- SIMULAÇÃO DE DADOS (Se não houver registros, mostra exemplos) ---
            if (result.Count == 0)
            {
                result.Add(new DiarioCampo { 
                    IdDCampo = 0, // ID 0 indica exemplo
                    AtorId = atorId,
                    Data = DateTime.Now, 
                    Descricao = "EXEMPLO 1: Realizada visita domiciliar. O ator relatou melhorias na convivência familiar. \n• AÇÃO: Roda de conversa | EIXO: Convivência | ATOR: Maria\n• AÇÃO: Encaminhamento | EIXO: Saúde | ATOR: João", 
                    DtCriacao = DateTime.Now 
                });
                result.Add(new DiarioCampo { 
                    IdDCampo = 0, 
                    AtorId = atorId,
                    Data = DateTime.Now.AddDays(-2), 
                    Descricao = "EXEMPLO 2: Participação na oficina de artesanato. Demonstrou grande interesse e habilidade manual.", 
                    DtCriacao = DateTime.Now.AddDays(-2) 
                });
                result.Add(new DiarioCampo { 
                    IdDCampo = 0, 
                    AtorId = atorId,
                    Data = DateTime.Now.AddDays(-5), 
                    Descricao = "EXEMPLO 3: Atendimento individual para atualização de cadastro. \n• AÇÃO: Atualização Cadastral | EIXO: Assistência | ATOR: Ana", 
                    DtCriacao = DateTime.Now.AddDays(-5) 
                });
            }
            // ---------------------------------------------------------------------

            return View(result);
        }

        // ... (Métodos Create e Edit continuam iguais aos que mandei anteriormente) ...
        
        public async Task<IActionResult> Create()
        {
            ViewBag.AtorList = new SelectList(await _context.Atores.OrderBy(a => a.Nome).ToListAsync(), "IdAtores", "Nome");
            ViewBag.EixosData = await _context.Eixos.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiarioCampo diario)
        {
            if (ModelState.IsValid)
            {
                diario.DtCriacao = DateTime.Now;
                diario.DtModificacao = DateTime.Now;
                _context.Add(diario);
                await _context.SaveChangesAsync();
                // Redireciona mantendo o filtro do ator
                return RedirectToAction(nameof(Index), new { atorId = diario.AtorId });
            }
            ViewBag.AtorList = new SelectList(await _context.Atores.OrderBy(a => a.Nome).ToListAsync(), "IdAtores", "Nome", diario.AtorId);
            ViewBag.EixosData = await _context.Eixos.ToListAsync();
            return View(diario);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || id == 0) return NotFound(); // Impede editar os exemplos (ID 0)
            var diario = await _context.DiariosCampo.FindAsync(id);
            if (diario == null) return NotFound();

            ViewBag.AtorList = new SelectList(await _context.Atores.OrderBy(a => a.Nome).ToListAsync(), "IdAtores", "Nome", diario.AtorId);
            ViewBag.EixosData = await _context.Eixos.ToListAsync();
            return View(diario);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiarioCampo diario)
        {
            if (id != diario.IdDCampo) return NotFound();
            if (ModelState.IsValid)
            {
                try {
                    var dbDiario = await _context.DiariosCampo.FindAsync(id);
                    if(dbDiario != null) {
                        dbDiario.AtorId = diario.AtorId;
                        dbDiario.Data = diario.Data;
                        dbDiario.Descricao = diario.Descricao;
                        dbDiario.DtModificacao = DateTime.Now;
                        _context.Update(dbDiario);
                        await _context.SaveChangesAsync();
                    }
                } catch (DbUpdateConcurrencyException) {
                    if (!_context.DiariosCampo.Any(e => e.IdDCampo == id)) return NotFound();
                    else throw;
                }
                return RedirectToAction(nameof(Index), new { atorId = diario.AtorId });
            }
            ViewBag.AtorList = new SelectList(await _context.Atores.OrderBy(a => a.Nome).ToListAsync(), "IdAtores", "Nome", diario.AtorId);
            ViewBag.EixosData = await _context.Eixos.ToListAsync();
            return View(diario);
        }
    }
}