using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Empodera.Data;
using Empodera.Models;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace InsEmpodera.Controllers
{
    public class FichaPrimeiroContatoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FichaPrimeiroContatoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FichaPrimeiroContato
        public async Task<IActionResult> Index(string searchTerm)
        {
            var fichas = _context.FichasPrimeiroContato
                .Include(f => f.Ator)
                .AsQueryable();

            var lista = await fichas.ToListAsync();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView(lista);

            return View(lista);
        }

        // GET: FichaPrimeiroContato/Create
        public IActionResult Create()
        {
            ViewBag.Atores = new SelectList(_context.Atores, "IdAtores", "Nome");
            return View();
        }

        // POST: FichaPrimeiroContato/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FichaPrimeiroContato ficha)
        {
            if (ModelState.IsValid)
            {
                ficha.DtCriacao = DateTime.Now;
                ficha.DtModificacao = DateTime.Now;

                _context.Add(ficha);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Atores = new SelectList(_context.Atores, "IdAtores", "Nome", ficha.FkIdUsuario);
            return View(ficha);
        }

        // GET: FichaPrimeiroContato/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ficha = await _context.FichasPrimeiroContato.FindAsync(id);
            if (ficha == null) return NotFound();

            ViewBag.Atores = new SelectList(_context.Atores, "IdAtores", "Nome", ficha.FkIdUsuario);

            return View(ficha);
        }

        // POST: FichaPrimeiroContato/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FichaPrimeiroContato ficha)
        {
            if (id != ficha.IdFicha) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    ficha.DtModificacao = DateTime.Now;

                    _context.Update(ficha);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FichaExists(ficha.IdFicha))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Atores = new SelectList(_context.Atores, "IdAtores", "Nome", ficha.FKidAtores);
            return View(ficha);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ficha = await _context.FichasPrimeiroContato
                .Include(f => f.Ator)
                .FirstOrDefaultAsync(m => m.IdFicha == id);

            if (ficha == null) return NotFound();

            return View(ficha);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var ficha = await _context.FichasPrimeiroContato.FindAsync(id);
            if (ficha != null)
            {
                _context.FichasPrimeiroContato.Remove(ficha);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FichaExists(int id)
        {
            return _context.FichasPrimeiroContato.Any(e => e.IdFicha == id);
        }
    }
}
