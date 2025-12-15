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
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Index", "Account");
            }

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
                .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0")
                    && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato"))
                .FirstOrDefault();

            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeListar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            var fichas = _context.FichasPrimeiroContato
                .Include(f => f.Ator)
                .Include(f => f.Usuario)
                .Include(f => f.Comunidade)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                fichas = fichas.Where(f => f.Ator.Nome.Contains(search));
            }

            if (!string.IsNullOrEmpty(status))
            {
                fichas = fichas.Where(f => f.Status == status);
            }

            if (comunidade.HasValue)
            {
                fichas = fichas.Where(f => f.FkIdComunidade == comunidade.Value);
            }

            var lista = await fichas
                .OrderByDescending(f => f.DtContato)
                .ToListAsync();

            var comunidades = await _context.Comunidades
                .OrderBy(c => c.Nome)
                .Select(c => new SelectListItem
                {
                    Value = c.IdComunidade.ToString(),
                    Text = c.Nome
                })
                .ToListAsync();

            ViewBag.Comunidades = comunidades;
            return View(lista);
        }

        // GET: FichaPrimeiroContato/Create
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Index", "Account");
            }

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
                .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0")
                    && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato"))
                .FirstOrDefault();

            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            // Filtrar atores ativos
            ViewBag.Atores = new SelectList(
                _context.Atores
                    .Where(a => a.Ativo == "S")  // Filtra apenas atores ativos
                    .OrderBy(a => a.Nome),
                "IdAtores",
                "Nome"
            );

            // Filtrar comunidades ativas
            ViewBag.ComunidadesList = new SelectList(
                _context.Comunidades
                    .Where(c => c.Ativo == "S")
                    .OrderBy(c => c.Nome),
                "IdComunidade",
                "Nome"
            );

            return View();
        }

        // POST: FichaPrimeiroContato/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            FichaPrimeiroContato ficha,
            List<string>? SelectedCondicoes,
            List<string>? SelectedPeticoes,
            List<string>? SelectedRespostas,
            List<string>? SelectedResultados)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Index", "Account");
            }

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
                .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0")
                    && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato"))
                .FirstOrDefault();

            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            ficha.DtCriacao = DateTime.Now;
            ficha.DtModificacao = DateTime.Now;

            if (string.IsNullOrEmpty(ficha.Status))
            {
                ficha.Status = "EmProgresso";
            }

            ficha.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID") ?? "0");

            _context.FichasPrimeiroContato.Add(ficha);
            await _context.SaveChangesAsync();

            if (SelectedCondicoes != null && SelectedCondicoes.Any())
            {
                foreach (var cond in SelectedCondicoes)
                {
                    _context.FichaCondicoes.Add(new FichaCondicoes
                    {
                        FkIdFicha = ficha.IdFicha,
                        Cond = cond
                    });
                }
            }

            if (SelectedPeticoes != null && SelectedPeticoes.Any())
            {
                foreach (var pet in SelectedPeticoes)
                {
                    _context.FichaPeticoes.Add(new FichaPeticoes
                    {
                        FkIdFicha = ficha.IdFicha,
                        Pet = pet
                    });
                }
            }

            if (SelectedRespostas != null && SelectedRespostas.Any())
            {
                foreach (var resp in SelectedRespostas)
                {
                    _context.FichaRespostas.Add(new FichaResp
                    {
                        FkIdFicha = ficha.IdFicha,
                        Resp = resp
                    });
                }
            }

            if (SelectedResultados != null && SelectedResultados.Any())
            {
                foreach (var result in SelectedResultados)
                {
                    _context.FichaResultados.Add(new FichaResult
                    {
                        FkIdFicha = ficha.IdFicha,
                        Result = result
                    });
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: FichaPrimeiroContato/Edit/5
        // GET: FichaPrimeiroContato/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ficha = await _context.FichasPrimeiroContato
                .Include(f => f.Comunidade)
                .Include(f => f.FichaPeticoes)
                .Include(f => f.FichaCondicoes)
                .Include(f => f.FichaRespostas)
                .Include(f => f.FichaResultados)
                .FirstOrDefaultAsync(f => f.IdFicha == id);

            if (ficha == null) return NotFound();

            // Filtrar atores ativos
            ViewBag.Atores = new SelectList(
                _context.Atores
                    .Where(a => a.Ativo == "S")
                    .OrderBy(a => a.Nome),
                "IdAtores",
                "Nome",
                ficha.FKidAtores
            );

            // Filtrar comunidades ativas
            ViewBag.ComunidadesList = new SelectList(
                _context.Comunidades
                    .Where(c => c.Ativo == "S")
                    .OrderBy(c => c.Nome),
                "IdComunidade",
                "Nome",
                ficha.FkIdComunidade
            );

            ViewBag.SelectedCondicoes = ficha.FichaCondicoes?.Select(c => c.Cond).ToList() ?? new List<string>();
            ViewBag.SelectedPeticoes = ficha.FichaPeticoes?.Select(p => p.Pet).ToList() ?? new List<string>();
            ViewBag.SelectedRespostas = ficha.FichaRespostas?.Select(r => r.Resp).ToList() ?? new List<string>();
            ViewBag.SelectedResultados = ficha.FichaResultados?.Select(res => res.Result).ToList() ?? new List<string>();

            return View(ficha);
        }

        // POST: FichaPrimeiroContato/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            FichaPrimeiroContato ficha,
            List<string>? SelectedCondicoes,
            List<string>? SelectedPeticoes,
            List<string>? SelectedRespostas,
            List<string>? SelectedResultados)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Index", "Account");
            }

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
                .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0")
                    && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato"))
                .FirstOrDefault();

            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            if (id != ficha.IdFicha)
                return NotFound();

            try
            {
                // ✅ CORREÇÃO PRINCIPAL: Buscar ficha existente sem rastreamento
                var fichaExistente = await _context.FichasPrimeiroContato
                    .AsNoTracking()
                    .FirstOrDefaultAsync(f => f.IdFicha == id);

                if (fichaExistente == null)
                    return NotFound();

                // ✅ Preservar campos importantes
                ficha.DtCriacao = fichaExistente.DtCriacao;
                ficha.FkIdUsuario = fichaExistente.FkIdUsuario;
                ficha.DtModificacao = DateTime.Now;

                // ✅ Atualizar a ficha principal
                _context.Update(ficha);
                await _context.SaveChangesAsync();

                // ✅ Atualizar condições
                var condicoesExistentes = _context.FichaCondicoes.Where(c => c.FkIdFicha == id);
                _context.FichaCondicoes.RemoveRange(condicoesExistentes);

                if (SelectedCondicoes != null && SelectedCondicoes.Any())
                {
                    foreach (var cond in SelectedCondicoes)
                    {
                        _context.FichaCondicoes.Add(new FichaCondicoes
                        {
                            FkIdFicha = id,
                            Cond = cond
                        });
                    }
                }

                // ✅ Atualizar petições
                var peticoesExistentes = _context.FichaPeticoes.Where(p => p.FkIdFicha == id);
                _context.FichaPeticoes.RemoveRange(peticoesExistentes);

                if (SelectedPeticoes != null && SelectedPeticoes.Any())
                {
                    foreach (var pet in SelectedPeticoes)
                    {
                        _context.FichaPeticoes.Add(new FichaPeticoes
                        {
                            FkIdFicha = id,
                            Pet = pet
                        });
                    }
                }

                // ✅ Atualizar respostas
                var respostasExistentes = _context.FichaRespostas.Where(r => r.FkIdFicha == id);
                _context.FichaRespostas.RemoveRange(respostasExistentes);

                if (SelectedRespostas != null && SelectedRespostas.Any())
                {
                    foreach (var resp in SelectedRespostas)
                    {
                        _context.FichaRespostas.Add(new FichaResp
                        {
                            FkIdFicha = id,
                            Resp = resp
                        });
                    }
                }

                // ✅ Atualizar resultados
                var resultadosExistentes = _context.FichaResultados.Where(r => r.FkIdFicha == id);
                _context.FichaResultados.RemoveRange(resultadosExistentes);

                if (SelectedResultados != null && SelectedResultados.Any())
                {
                    foreach (var result in SelectedResultados)
                    {
                        _context.FichaResultados.Add(new FichaResult
                        {
                            FkIdFicha = id,
                            Result = result
                        });
                    }
                }

                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.FichasPrimeiroContato.Any(e => e.IdFicha == ficha.IdFicha))
                    return NotFound();
                else
                    throw;
            }
            catch (Exception ex)
            {
                // Log para debug em caso de erro
                Console.WriteLine($"Erro ao atualizar ficha: {ex.Message}");
                if (ex.InnerException != null)
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Concluir ficha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Concluir(int id, string returnUrl)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
                .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0")
                    && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato"))
                .FirstOrDefault();

            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            var ficha = await _context.FichasPrimeiroContato.FindAsync(id);
            if (ficha == null)
            {
                return NotFound();
            }

            ficha.Status = "Concluida";
            ficha.DtModificacao = DateTime.Now;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Abandonar ficha
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abandonar(int id, string returnUrl)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
                .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0")
                    && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato"))
                .FirstOrDefault();

            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            var ficha = await _context.FichasPrimeiroContato.FindAsync(id);
            if (ficha == null)
            {
                return NotFound();
            }

            ficha.Status = "Abandonada";
            ficha.DtModificacao = DateTime.Now;

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: FichaPrimeiroContato/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
                .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0")
                    && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato"))
                .FirstOrDefault();

            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeDeletar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            if (id == null) return NotFound();

            var ficha = await _context.FichasPrimeiroContato
                .Include(f => f.Ator)
                .Include(f => f.Comunidade)
                .FirstOrDefaultAsync(m => m.IdFicha == id);

            if (ficha == null) return NotFound();

            return View(ficha);
        }

        // POST: FichaPrimeiroContato/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
                .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0")
                    && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato"))
                .FirstOrDefault();

            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeDeletar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

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