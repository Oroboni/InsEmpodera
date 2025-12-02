using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Empodera.Data;
using Empodera.Models;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Empodera.Controllers
{
    public class DiarioCampoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiarioCampoController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string search)
        {
            var q = _context.DiariosCampo
                .Include(d => d.Comunidade)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                q = q.Where(d =>
                    d.Descricao.Contains(search) ||
                    d.Localizacao.Contains(search) ||
                    d.Comunidade.Nome.Contains(search)
                );
            }

            var list = await q
                .OrderByDescending(d => d.DtCriacao)
                .ToListAsync();

            list.ForEach(d =>
            {
                if (!string.IsNullOrEmpty(d.Descricao) && d.Descricao.Length > 50)
                    d.Descricao = d.Descricao.Substring(0, 35) + "...";
            });

            return View(list);
        }


        public IActionResult Create()
        {
            PreencherViewBags();
            return View();
        }

        private void PreencherViewBags()
        {
            ViewBag.Comunidades = new List<dynamic>
            {
                new { IdComunidade = 1, Nome = "Comunidade A" },
                new { IdComunidade = 2, Nome = "Comunidade B" }
            };

            ViewBag.Eixos = new List<dynamic>
            {
                new { IdEixo = 1, Nome = "Cidadania" },
                new { IdEixo = 2, Nome = "Educação" },
                new { IdEixo = 3, Nome = "Esporte" }
            };

            ViewBag.Atividades = new List<dynamic>
            {
                new { IdAtividade = 1, Nome = "Oficina" },
                new { IdAtividade = 2, Nome = "Acompanhamento" }
            };

            ViewBag.Acoes = new List<dynamic>
            {
                new { IdAcao = 1, Nome = "Visita" },
                new { IdAcao = 2, Nome = "Reunião" }
            };
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiarioCampo diarioCampo)
        {
            if (ModelState.IsValid)
            {
                diarioCampo.DtCriacao = DateTime.Now;
                diarioCampo.DtModificacao = diarioCampo.DtCriacao;

                _context.Add(diarioCampo);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            PreencherViewBags();
            return View(diarioCampo);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var diarioCampo = await _context.DiariosCampo.FindAsync(id);
            if (diarioCampo == null) return NotFound();

            PreencherViewBags();
            return View(diarioCampo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiarioCampo diarioCampo)
        {
            if (id != diarioCampo.IdDCampo)
                return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    diarioCampo.DtModificacao = DateTime.Now;
                    _context.Update(diarioCampo);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiarioCampoExists(diarioCampo.IdDCampo))
                        return NotFound();

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            PreencherViewBags();
            return View(diarioCampo);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var diarioCampo = await _context.DiariosCampo
                .FirstOrDefaultAsync(d => d.IdDCampo == id);

            if (diarioCampo == null) return NotFound();

            return View(diarioCampo);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var diarioCampo = await _context.DiariosCampo
                .FirstOrDefaultAsync(d => d.IdDCampo == id);

            if (diarioCampo == null) return NotFound();

            return View(diarioCampo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var diarioCampo = await _context.DiariosCampo.FindAsync(id);

            if (diarioCampo != null)
            {
                _context.DiariosCampo.Remove(diarioCampo);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool DiarioCampoExists(int id)
        {
            return _context.DiariosCampo.Any(e => e.IdDCampo == id);
        }
    }
}
