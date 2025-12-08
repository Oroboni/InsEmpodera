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
        public async Task<IActionResult> Index(string search, string status, int? comunidade)
        {
            var fichas = _context.FichasPrimeiroContato
                .Include(f => f.Ator)
                .Include(f => f.FichaComunidades)
                    .ThenInclude(fc => fc.Comunidade)
                .AsQueryable();

            // Filtro por status
            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<StatusFicha>(status, true, out var statusEnum))
                {
                    fichas = fichas.Where(f => f.Status == statusEnum);
                }
            }

            // Filtro por comunidade (usando a tabela de junção)
            if (comunidade.HasValue && comunidade > 0)
            {
                fichas = fichas.Where(f => 
                    f.FichaComunidades.Any(fc => fc.FkIdComunidade == comunidade.Value)
                );
            }

            // Filtro por busca - APENAS POR NOME DO ATOR
            if (!string.IsNullOrEmpty(search))
            {
                fichas = fichas.Where(f => 
                    f.Ator.Nome.Contains(search)
                );
            }

            var lista = await fichas.OrderByDescending(f => f.DtContato).ToListAsync();

            // Obter comunidades ativas
            var comunidades = await _context.Comunidades
                .Where(a => a.Ativo == "S")
                .OrderBy(c => c.Nome)
                .ToListAsync();

            ViewBag.Comunidades = new SelectList(
                comunidades,
                "IdComunidade",
                "Nome",
                comunidade
            );

            ViewData["CurrentFilter"] = search;
            ViewData["CurrentStatus"] = status;
            ViewData["CurrentComunidade"] = comunidade;

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                return PartialView(lista);

            return View(lista);
        }

        // GET: FichaPrimeiroContato/Create
        public IActionResult Create()
        {
            ViewBag.Atores = new SelectList(_context.Atores, "IdAtores", "Nome");
            
            ViewBag.ComunidadesList = new SelectList(
                _context.Comunidades.Where(c => c.Ativo == "S").OrderBy(c => c.Nome),
                "IdComunidade",
                "Nome"
            );
            
            return View();
        }

        // POST: FichaPrimeiroContato/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(FichaPrimeiroContato ficha, int? comunidadeId)
        {
            if (ModelState.IsValid)
            {
                ficha.DtCriacao = DateTime.Now;
                ficha.DtModificacao = DateTime.Now;

                _context.Add(ficha);
                await _context.SaveChangesAsync();

                // Se uma comunidade foi selecionada, criar a relação
                if (comunidadeId.HasValue && comunidadeId > 0)
                {
                    var fichaComunidade = new Ficha1oContatoComunidade
                    {
                        IdFicha = ficha.IdFicha,
                        FkIdComunidade = comunidadeId.Value
                    };
                    _context.Ficha1oContatoComunidades.Add(fichaComunidade);
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            ViewBag.Atores = new SelectList(_context.Atores, "IdAtores", "Nome", ficha.FKidAtores);
            ViewBag.ComunidadesList = new SelectList(
                _context.Comunidades.Where(c => c.Ativo == "S").OrderBy(c => c.Nome),
                "IdComunidade",
                "Nome"
            );
            return View(ficha);
        }

        // GET: FichaPrimeiroContato/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ficha = await _context.FichasPrimeiroContato
                .Include(f => f.FichaComunidades)
                .FirstOrDefaultAsync(f => f.IdFicha == id);
                
            if (ficha == null) return NotFound();

            ViewBag.Atores = new SelectList(_context.Atores, "IdAtores", "Nome", ficha.FKidAtores);
            
            // Obter a comunidade associada (se houver)
            var fichaComunidade = await _context.Ficha1oContatoComunidades
                .FirstOrDefaultAsync(fc => fc.IdFicha == id);
            
            ViewBag.ComunidadesList = new SelectList(
                _context.Comunidades.Where(c => c.Ativo == "S").OrderBy(c => c.Nome),
                "IdComunidade",
                "Nome",
                fichaComunidade?.FkIdComunidade
            );

            return View(ficha);
        }

        // POST: FichaPrimeiroContato/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, FichaPrimeiroContato ficha, int? comunidadeId)
        {
            if (id != ficha.IdFicha) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    ficha.DtModificacao = DateTime.Now;

                    _context.Update(ficha);
                    await _context.SaveChangesAsync();

                    // Atualizar a relação com comunidade
                    var fichaComunidade = await _context.Ficha1oContatoComunidades
                        .FirstOrDefaultAsync(fc => fc.IdFicha == id);

                    if (comunidadeId.HasValue && comunidadeId > 0)
                    {
                        if (fichaComunidade == null)
                        {
                            // Criar nova relação
                            fichaComunidade = new Ficha1oContatoComunidade
                            {
                                IdFicha = id,
                                FkIdComunidade = comunidadeId.Value
                            };
                            _context.Ficha1oContatoComunidades.Add(fichaComunidade);
                        }
                        else
                        {
                            // Atualizar relação existente
                            fichaComunidade.FkIdComunidade = comunidadeId.Value;
                            _context.Update(fichaComunidade);
                        }
                    }
                    else if (fichaComunidade != null)
                    {
                        // Remover relação se nenhuma comunidade foi selecionada
                        _context.Ficha1oContatoComunidades.Remove(fichaComunidade);
                    }

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
            
            ViewBag.ComunidadesList = new SelectList(
                _context.Comunidades.Where(c => c.Ativo == "S").OrderBy(c => c.Nome),
                "IdComunidade",
                "Nome",
                comunidadeId
            );
            
            return View(ficha);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Concluir(int id, string returnUrl = null)
        {
            var ficha = await _context.FichasPrimeiroContato.FindAsync(id);
            if (ficha == null)
            {
                return NotFound();
            }

            ficha.Status = StatusFicha.Concluida;
            ficha.DtModificacao = DateTime.Now;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abandonar(int id, string returnUrl = null)
        {
            var ficha = await _context.FichasPrimeiroContato.FindAsync(id);
            if (ficha == null)
            {
                return NotFound();
            }

            ficha.Status = StatusFicha.Abandonada;
            ficha.DtModificacao = DateTime.Now;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var ficha = await _context.FichasPrimeiroContato
                .Include(f => f.Ator)
                .Include(f => f.FichaComunidades)
                    .ThenInclude(fc => fc.Comunidade)
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
                var fichaComunidades = _context.Ficha1oContatoComunidades
                    .Where(fc => fc.IdFicha == id);
                _context.Ficha1oContatoComunidades.RemoveRange(fichaComunidades);
                
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