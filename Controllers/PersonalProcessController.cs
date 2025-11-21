using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Empodera.Data;
using Empodera.Models;

namespace InsEmpodera.Controllers
{
    public class PersonalProcessController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PersonalProcessController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // LISTAGEM / BUSCA
        // ==========================================================
        public async Task<IActionResult> Index(int? atorId, string searchQuery)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

            ViewBag.AtorList = new SelectList(
                await _context.Atores.OrderBy(a => a.Nome).ToListAsync(),
                "IdAtores",
                "Nome",
                atorId
            );

            ViewBag.SelectedAtorId = atorId;
            ViewBag.SearchQuery = searchQuery;

            var query = _context.DiariosCampo
                .Include(d => d.Ator)
                .Include(d => d.Comunidade)
                .AsQueryable();

            if (atorId.HasValue && atorId > 0)
                query = query.Where(d => d.AtorId == atorId.Value);
            else
                return View(new List<DiarioCampo>()); // Sem ator → lista vazia

            if (!string.IsNullOrEmpty(searchQuery))
                query = query.Where(d => d.Descricao.Contains(searchQuery));

            var diarios = await query
                .OrderByDescending(d => d.DataCriacao)
                .ToListAsync();

            return View(diarios);
        }

        // ==========================================================
        // CRIAÇÃO - GET
        // ==========================================================
        public async Task<IActionResult> Create(int? atorId)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

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

            return View(new DiarioCampo
            {
                AtorId = atorId ?? 0,
                DataCriacao = DateTime.Now
            });
        }

        // ==========================================================
        // CRIAÇÃO - POST
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiarioCampo diario, int[] eixosIds)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

            if (diario.AtorId <= 0)
                ModelState.AddModelError("AtorId", "Selecione um ator.");

            if (!ModelState.IsValid)
            {
                await CarregarListas(diario.AtorId);
                return View(diario);
            }

            diario.DataCriacao = DateTime.Now;
            diario.UltimaAtualizacao = DateTime.Now;

            _context.Add(diario);
            await _context.SaveChangesAsync();

            foreach (var eixoId in eixosIds)
            {
                _context.Add(new DiarioEixo
                {
                    DiarioId = diario.Id,
                    EixoId = eixoId
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { atorId = diario.AtorId });
        }

        // ==========================================================
        // EDITAR - GET
        // ==========================================================
        public async Task<IActionResult> Edit(int id)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

            var diario = await _context.DiariosCampo
                .Include(d => d.Eixos)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (diario == null) return NotFound();

            await CarregarListas(diario.AtorId);

            return View(diario);
        }

        // ==========================================================
        // EDITAR - POST
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiarioCampo diario, int[] eixosIds)
        {
            if (id != diario.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                await CarregarListas(diario.AtorId);
                return View(diario);
            }

            var existente = await _context.DiariosCampo
                .Include(d => d.Eixos)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (existente == null) return NotFound();

            existente.Descricao = diario.Descricao;
            existente.CEP = diario.CEP;
            existente.Endereco = diario.Endereco;
            existente.UltimaAtualizacao = DateTime.Now;

            _context.DiarioEixos.RemoveRange(existente.Eixos);

            foreach (var eid in eixosIds)
            {
                existente.Eixos.Add(new DiarioEixo
                {
                    DiarioId = existente.Id,
                    EixoId = eid
                });
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index), new { atorId = existente.AtorId });
        }

        // ==========================================================
        // HELPER
        // ==========================================================
        private async Task CarregarListas(int atorId)
        {
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
        }
    }
}
