using Empodera.Services;
using Empodera.Services.Validacao;
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
        public async Task<IActionResult> Index(string? search, string? status, int? comunidade)
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Index", "Account");
            }

            var PodeDiario = _context.Usuarios.Include(c => c.Perfil).ThenInclude(p => p.Permissoes)
                .Where(u => u.IdUsuario == int.Parse(HttpContext.Session.GetString("ID") ?? "0")
                    && u.Perfil.Permissoes.Any(p => p.Modulo == "Ficha1Contato"))
                .FirstOrDefault();

            if (!PodeDiario.CanList("Ficha1Contato"))
            {
                return StatusCode(StatusCodes.Status403Forbidden);
            }

            var fichas = _context.FichasPrimeiroContato
                .Include(f => f.Ator)
                .Include(f => f.Usuario)
                .Include(f => f.Comunidade)
                .AsQueryable().Where(a => a.Ator.Ativo != "N");

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
                    Value = c.Id_Comunidade.ToString(),
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

            if (!PodeDiario.CanCreate("Ficha1Contato"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            CarregarListas(null);
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

            if (!PodeDiario.CanCreate("Ficha1Contato"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            if (!await ValidarFichaAsync(ficha))
            {
                CarregarListas(ficha, SelectedCondicoes, SelectedPeticoes, SelectedRespostas, SelectedResultados);
                return View(ficha);
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
            this.AvisarSucesso("Ficha salva com sucesso.");
            return RedirectToAction(nameof(Index));
        }

        // Comunidades e atores ativos dos formulários; cada ator leva as comunidades dele (filtro na tela).
        private void CarregarListas(FichaPrimeiroContato? ficha,
            List<string>? condicoes = null, List<string>? peticoes = null,
            List<string>? respostas = null, List<string>? resultados = null)
        {
            ViewBag.Atores = new SelectList(_context.Atores.OrderBy(a => a.Nome), "IdAtores", "Nome", ficha?.FK_id_Atores);
            ViewBag.AtoresComunidades = _context.AtorComunidades.AsNoTracking()
                .Where(ac => ac.Ator.Ativo != "N")
                .Select(ac => new { ac.FK_id_Atores, ac.FkIdComunidade })
                .AsEnumerable()
                .GroupBy(ac => ac.FK_id_Atores)
                .ToDictionary(g => g.Key, g => string.Join(",", g.Select(ac => ac.FkIdComunidade)));
            ViewBag.ComunidadesList = new SelectList(_context.Comunidades.OrderBy(c => c.Nome), "Id_Comunidade", "Nome", ficha?.FkIdComunidade);
            ViewBag.SelectedCondicoes = condicoes ?? new List<string>();
            ViewBag.SelectedPeticoes = peticoes ?? new List<string>();
            ViewBag.SelectedRespostas = respostas ?? new List<string>();
            ViewBag.SelectedResultados = resultados ?? new List<string>();
        }

        // Regras da ficha no servidor (antes, uma ficha incompleta estourava erro 500 no banco).
        private async Task<bool> ValidarFichaAsync(FichaPrimeiroContato ficha)
        {
            var errosDeConversao = ModelState
                .Where(item => item.Key is nameof(FichaPrimeiroContato.DtContato) or nameof(FichaPrimeiroContato.HoraContato)
                    && item.Value?.Errors.Count > 0)
                .Select(item => item.Key)
                .ToHashSet();
            ModelState.Clear();

            await ValidacaoCadastros.ComunidadeAtivaAsync(_context, ModelState, ficha.FkIdComunidade, nameof(FichaPrimeiroContato.FkIdComunidade));
            if (ficha.FK_id_Atores <= 0 || !await _context.Atores.AnyAsync(a => a.IdAtores == ficha.FK_id_Atores))
                ModelState.AddModelError(nameof(FichaPrimeiroContato.FK_id_Atores), ValidacaoCadastros.Obrigatorio(nameof(FichaPrimeiroContato.FK_id_Atores)));

            if (ficha.DtContato == default || errosDeConversao.Contains(nameof(FichaPrimeiroContato.DtContato)))
                ModelState.AddModelError(nameof(FichaPrimeiroContato.DtContato), ValidacaoCadastros.Obrigatorio(nameof(FichaPrimeiroContato.DtContato)));
            else if (ficha.DtContato.Date > DateTime.Today)
                ModelState.AddModelError(nameof(FichaPrimeiroContato.DtContato),
                    string.Format(MensagensValidacao.DataFutura, ValidacaoAmigavelProvider.NomeDoCampo(nameof(FichaPrimeiroContato.DtContato))));
            if (errosDeConversao.Contains(nameof(FichaPrimeiroContato.HoraContato)) || (HttpContext?.Request.HasFormContentType == true && string.IsNullOrWhiteSpace(Request.Form["HoraContato"])))
                ModelState.AddModelError(nameof(FichaPrimeiroContato.HoraContato), ValidacaoCadastros.Obrigatorio(nameof(FichaPrimeiroContato.HoraContato)));

            if (string.IsNullOrWhiteSpace(ficha.CEstabeleceu))
                ModelState.AddModelError(nameof(FichaPrimeiroContato.CEstabeleceu), "Informe como o contato foi estabelecido.");
            if (string.IsNullOrWhiteSpace(ficha.NovoParceiro))
                ModelState.AddModelError(nameof(FichaPrimeiroContato.NovoParceiro), "Informe se é um novo parceiro.");
            if (string.IsNullOrWhiteSpace(ficha.FornecidoParceiro))
                ModelState.AddModelError(nameof(FichaPrimeiroContato.FornecidoParceiro), "Informe se o contato foi fornecido por um parceiro.");

            // Tamanhos e intervalos declarados no modelo (StringLength, Range).
            var resultados = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                ficha, new System.ComponentModel.DataAnnotations.ValidationContext(ficha), resultados, validateAllProperties: true);
            foreach (var r in resultados)
                foreach (var membro in r.MemberNames.DefaultIfEmpty(string.Empty))
                    ModelState.AddModelError(membro, r.ErrorMessage ?? string.Empty);

            return ModelState.IsValid;
        }

        // GET: FichaPrimeiroContato/Edit/5
        // GET: FichaPrimeiroContato/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

            var loggedUserId = int.Parse(HttpContext.Session.GetString("ID") ?? "0");
            var loggedUser = await _context.Usuarios
                .Include(user => user.Perfil)
                .ThenInclude(profile => profile.Permissoes)
                .FirstOrDefaultAsync(user => user.IdUsuario == loggedUserId);
            if (!loggedUser.CanUpdate("Ficha1Contato"))
                return RedirectToAction(nameof(Index));

            if (id == null) return NotFound();

            var ficha = await _context.FichasPrimeiroContato
                .Include(f => f.Comunidade)
                .Include(f => f.FichaPeticoes)
                .Include(f => f.FichaCondicoes)
                .Include(f => f.FichaRespostas)
                .Include(f => f.FichaResultados)
                .FirstOrDefaultAsync(f => f.IdFicha == id);

            if (ficha == null) return NotFound();

            CarregarListas(ficha,
                ficha.FichaCondicoes?.Select(c => c.Cond).ToList(),
                ficha.FichaPeticoes?.Select(p => p.Pet).ToList(),
                ficha.FichaRespostas?.Select(r => r.Resp).ToList(),
                ficha.FichaResultados?.Select(res => res.Result).ToList());
            ViewBag.NomeAtor = await _context.Atores.Where(a => a.IdAtores == ficha.FK_id_Atores).Select(a => a.Nome).FirstOrDefaultAsync();

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

            if (!PodeDiario.CanUpdate("Ficha1Contato"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            if (id != ficha.IdFicha)
                return NotFound();

            if (!await ValidarFichaAsync(ficha))
            {
                CarregarListas(ficha, SelectedCondicoes, SelectedPeticoes, SelectedRespostas, SelectedResultados);
                ViewBag.NomeAtor = await _context.Atores.Where(a => a.IdAtores == ficha.FK_id_Atores).Select(a => a.Nome).FirstOrDefaultAsync();
                return View(ficha);
            }

            try
            {
                var fichaExistente = await _context.FichasPrimeiroContato
                    .FirstOrDefaultAsync(f => f.IdFicha == id);

                if (fichaExistente == null)
                    return NotFound();

                // Update only fields editable by the form. Identity and audit data are preserved.
                fichaExistente.FK_id_Atores = ficha.FK_id_Atores;
                fichaExistente.Endereco = ficha.Endereco;
                fichaExistente.Emprego = ficha.Emprego;
                fichaExistente.CEstabeleceu = ficha.CEstabeleceu;
                fichaExistente.NovoParceiro = ficha.NovoParceiro;
                fichaExistente.FornecidoParceiro = ficha.FornecidoParceiro;
                fichaExistente.Telefone = ficha.Telefone;
                fichaExistente.LContato = ficha.LContato;
                fichaExistente.EstaFamiliar = ficha.EstaFamiliar;
                fichaExistente.EstruFamiliar = ficha.EstruFamiliar;
                fichaExistente.NFIlhos = ficha.NFIlhos;
                fichaExistente.NFilhas = ficha.NFilhas;
                fichaExistente.AEscolar = ficha.AEscolar;
                fichaExistente.SLer = ficha.SLer;
                fichaExistente.SCalc = ficha.SCalc;
                fichaExistente.SComp = ficha.SComp;
                fichaExistente.QReabili = ficha.QReabili;
                fichaExistente.LTrat = ficha.LTrat;
                fichaExistente.Coment = ficha.Coment;
                fichaExistente.DtContato = ficha.DtContato;
                fichaExistente.HoraContato = ficha.HoraContato;
                fichaExistente.FkIdComunidade = ficha.FkIdComunidade;
                fichaExistente.DtModificacao = DateTime.Now;
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

            this.AvisarSucesso("Alterações salvas com sucesso.");
            return RedirectToAction(nameof(Edit), new { id });
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

            if (!PodeDiario.CanUpdate("Ficha1Contato"))
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
            this.AvisarSucesso("Ficha concluída.");

            if (IsSafeLocalReturnUrl(returnUrl))
                return LocalRedirect(returnUrl);

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

            if (!PodeDiario.CanUpdate("Ficha1Contato"))
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
            this.AvisarSucesso("Ficha marcada como abandonada.");

            if (IsSafeLocalReturnUrl(returnUrl))
                return LocalRedirect(returnUrl);

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

            if (!PodeDiario.CanDelete("Ficha1Contato"))
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

            if (!PodeDiario.CanDelete("Ficha1Contato"))
            {
                return RedirectToAction("Index", "FichaPrimeiroContato");
            }

            var ficha = await _context.FichasPrimeiroContato.FindAsync(id);
            if (ficha != null)
            {
                _context.FichasPrimeiroContato.Remove(ficha);
                await _context.SaveChangesAsync();
                this.AvisarSucesso("Ficha excluída.");
            }

            return RedirectToAction(nameof(Index));
        }

        private static bool IsSafeLocalReturnUrl(string? returnUrl) =>
            !string.IsNullOrWhiteSpace(returnUrl) &&
            returnUrl.StartsWith('/') &&
            !returnUrl.StartsWith("//", StringComparison.Ordinal) &&
            !returnUrl.StartsWith("/\\", StringComparison.Ordinal);
        private bool FichaExists(int id)
        {
            return _context.FichasPrimeiroContato.Any(e => e.IdFicha == id);
        }
    }
}
