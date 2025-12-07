using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Empodera.Data;
using Empodera.Models;

namespace Empodera.Controllers
{
    public class FichaPrimeiroContatoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FichaPrimeiroContatoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FichaPrimeiroContato
        // No método Index do Controller
        public async Task<IActionResult> Index(string search, string status)
        {
            var fichas = _context.FichasPrimeiroContato
                .Include(f => f.Ator)
                .AsQueryable();

            // Filtro por status
            if (!string.IsNullOrEmpty(status))
            {
                var statusEnum = Enum.Parse<StatusFicha>(status);
                fichas = fichas.Where(f => f.Status == statusEnum);
            }

            // Filtro por busca
            if (!string.IsNullOrEmpty(search))
            {
                fichas = fichas.Where(f => f.Ator.Nome.Contains(search) ||
                                           f.Endereco.Contains(search));
            }

            var lista = await fichas.ToListAsync();

            ViewBag.Comunidades = new SelectList(
                await _context.Comunidades.Where(a => a.Ativo == "S").OrderBy(c => c.Nome).ToListAsync(),
                "IdComunidade",
                "Nome"
            );

            ViewData["CurrentFilter"] = search;

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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Concluir(int id)
        {
            var ficha = await _context.FichasPrimeiroContato.FindAsync(id);
            if (ficha == null)
            {
                return NotFound();
            }

            ficha.Status = StatusFicha.Concluida;
            ficha.DtModificacao = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abandonar(int id)
        {
            var ficha = await _context.FichasPrimeiroContato.FindAsync(id);
            if (ficha == null)
            {
                return NotFound();
            }

            ficha.Status = StatusFicha.Abandonada;
            ficha.DtModificacao = DateTime.Now;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
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
