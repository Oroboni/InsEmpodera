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
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato")).FirstOrDefault();
            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeListar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            var fichas = _context.FichasPrimeiroContato
                .Include(f => f.Ator)
                .Include(f => f.FichaComunidades)
                    .ThenInclude(fc => fc.Comunidade)
                .AsQueryable();

            // Filtro por status
            if (!string.IsNullOrEmpty(status))
            {
                fichas = fichas.Where(f => f.Status == status);
            }

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
        [HttpGet]
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Index", "Account");
            }

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato")).FirstOrDefault();
            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            ViewBag.Atores = new SelectList(_context.Atores, "IdAtores", "Nome");
            
            ViewBag.ComunidadesList = new SelectList(
                _context.Comunidades.Where(c => c.Ativo == "S").OrderBy(c => c.Nome),
                "IdComunidade",
                "Nome"
            );
            
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]        
        public async Task<IActionResult> Create(FichaPrimeiroContato ficha, int? comunidadeId, List<string>? SelectedCondicoes, List<string>? SelectedPeticoes, List<string>? SelectedRespostas, List<string>? SelectedResultados)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Index", "Account");
            }

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato")).FirstOrDefault();
            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeCriar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            Console.WriteLine($"FKidAtores recebido: {ficha.FKidAtores}");

            ViewBag.Atores = new SelectList(_context.Atores, "IdAtores", "Nome", ficha.FKidAtores);
            ViewBag.ComunidadesList = new SelectList(
                _context.Comunidades.Where(c => c.Ativo == "S").OrderBy(c => c.Nome),
                "IdComunidade", "Nome"
            );

            ficha.DtCriacao = DateTime.Now;
            ficha.DtModificacao = DateTime.Now;
            ficha.Status = "Em Progresso"; 
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

            if (comunidadeId.HasValue && comunidadeId > 0)
            {
                var fichaComunidade = new Ficha1oContatoComunidade
                {
                    IdFicha = ficha.IdFicha,
                    FkIdComunidade = comunidadeId.Value
                };
                _context.Ficha1oContatoComunidades.Add(fichaComunidade);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    

        // GET: FichaPrimeiroContato/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var ficha = await _context.FichasPrimeiroContato
                .Include(f => f.FichaPeticoes)
                .Include(f => f.FichaCondicoes)
                .Include(f => f.FichaRespostas)
                .Include(f => f.FichaResultados)
                .FirstOrDefaultAsync(f => f.IdFicha == id);

            if (ficha == null) return NotFound();

            ViewBag.Atores = new SelectList(_context.Atores, "IdAtores", "Nome", ficha.FKidAtores);

            // Obter comunidade associada
            var fichaComunidade = await _context.Ficha1oContatoComunidades
                .FirstOrDefaultAsync(fc => fc.IdFicha == id);

            ViewBag.ComunidadesList = new SelectList(
                _context.Comunidades.Where(c => c.Ativo == "S").OrderBy(c => c.Nome),
                "IdComunidade",
                "Nome",
                fichaComunidade?.FkIdComunidade
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
        public async Task<IActionResult> Edit(int id, FichaPrimeiroContato ficha, int? comunidadeId, List<string>? SelectedCondicoes, List<string>? SelectedPeticoes, List<string>? SelectedRespostas, List<string>? SelectedResultados)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Index", "Account");
            }

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato")).FirstOrDefault();
            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeAtualizar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            if (id != ficha.IdFicha)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.Atores = new SelectList(_context.Atores, "IdAtores", "Nome", ficha.FKidAtores);
                ViewBag.ComunidadesList = new SelectList(
                    _context.Comunidades.Where(c => c.Ativo == "S").OrderBy(c => c.Nome),
                    "IdComunidade",
                    "Nome",
                    comunidadeId
                );
                return View(ficha);
            }

            try
            {
                ficha.DtModificacao = DateTime.Now;
                _context.Update(ficha);
                await _context.SaveChangesAsync();

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

                var fichaComunidade = await _context.Ficha1oContatoComunidades
                    .FirstOrDefaultAsync(fc => fc.IdFicha == id);

                if (comunidadeId.HasValue && comunidadeId > 0)
                {
                    if (fichaComunidade == null)
                    {
                        fichaComunidade = new Ficha1oContatoComunidade
                        {
                            IdFicha = id,
                            FkIdComunidade = comunidadeId.Value
                        };
                        _context.Ficha1oContatoComunidades.Add(fichaComunidade);
                    }
                    else
                    {
                        fichaComunidade.FkIdComunidade = comunidadeId.Value;
                        _context.Update(fichaComunidade);
                    }
                }
                else if (fichaComunidade != null)
                {
                    _context.Ficha1oContatoComunidades.Remove(fichaComunidade);
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

            return RedirectToAction(nameof(Index));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Concluir(int id, string returnUrl)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato")).FirstOrDefault();
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Abandonar(int id, string returnUrl)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato")).FirstOrDefault();
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

        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");
            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato")).FirstOrDefault();
            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeDeletar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

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
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");
            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
            .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0") && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato")).FirstOrDefault();
            if (PodeDiario == null || PodeDiario.Perfil.Permissoes.Any(p => p.PodeDeletar == "N"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }
            
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