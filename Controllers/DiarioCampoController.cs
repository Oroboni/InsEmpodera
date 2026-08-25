using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Empodera.Data;
using Empodera.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Empodera.Controllers
{
    public class DiarioCampoController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DiarioCampoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================================
        // INDEX
        // ==========================================================
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

            // Verifica Permissão
            var usuarioLogado = await GetUsuarioLogadoAsync();
            if (usuarioLogado == null || !PermiteModulo(usuarioLogado, "DiariosCampo", "Listar"))
                return RedirectToAction("Index", "Home"); // Redireciona se não tiver acesso

            // 1. Carrega todos os diários (o filtro visual é feito via JavaScript na View)
            var diarios = await _context.DiariosCampo
                .Include(d => d.Comunidade)
                // .Include(d => d.DiarioEixos).ThenInclude(de => de.Eixo) // Descomente se precisar mostrar eixos na lista
                .OrderByDescending(d => d.Data)
                .ToListAsync();

            // 2. Preenche a lista para o Filtro de Comunidades (Dropdown do topo)
            // Pegamos apenas comunidades que existem no banco
            ViewBag.Comunidades = await _context.Comunidades
                .OrderBy(c => c.Nome)
                .Select(c => new { c.Nome }) 
                .Distinct()
                .ToListAsync();

            return View(diarios);
        }

        // ==========================================================
        // CREATE (GET)
        // ==========================================================
        public async Task<IActionResult> Create()
        {
            if (HttpContext.Session.GetString("Email") == null) return RedirectToAction("Index", "Account");

            var usuarioLogado = await GetUsuarioLogadoAsync();
            if (usuarioLogado == null || !PermiteModulo(usuarioLogado, "DiariosCampo", "Criar"))
                return RedirectToAction("Index");

            await PreencherViewBagsDoBanco(); // Usa dados REAIS
            return View();
        }

        // ==========================================================
        // CREATE (POST)
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiarioCampo diarioCampo, int[] EixosSelecionados)
        {
            if (HttpContext.Session.GetString("Email") == null) return RedirectToAction("Index", "Account");

            var usuarioLogado = await GetUsuarioLogadoAsync();
            if (usuarioLogado == null || !PermiteModulo(usuarioLogado, "DiariosCampo", "Criar"))
                return RedirectToAction(nameof(Index));

            // Preenche dados automáticos
            diarioCampo.DtCriacao = DateTime.Now;
            diarioCampo.DtModificacao = DateTime.Now;
            diarioCampo.FkIdUsuario = int.Parse(HttpContext.Session.GetString("ID") ?? "0");

            // Remove validações de navegação que podem dar erro
            ModelState.Remove("Comunidade");
            ModelState.Remove("Usuario");

            if (ModelState.IsValid)
            {
                _context.Add(diarioCampo);
                await _context.SaveChangesAsync();

                // Salvar Eixos
                if (EixosSelecionados != null)
                {
                    foreach (var eixoId in EixosSelecionados)
                    {
                        _context.DiarioEixos.Add(new DiarioEixo { FkIdDiario = diarioCampo.IdDCampo, FkIdEixo = eixoId });
                    }
                    await _context.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Index));
            }

            await PreencherViewBagsDoBanco();
            return View(diarioCampo);
        }

        // ==========================================================
        // EDIT (GET)
        // ==========================================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("Email") == null) return RedirectToAction("Index", "Account");

            if (id == null) return NotFound();

            var usuarioLogado = await GetUsuarioLogadoAsync();
            if (usuarioLogado == null || !PermiteModulo(usuarioLogado, "DiariosCampo", "Atualizar"))
                return RedirectToAction("Index");

            var diarioCampo = await _context.DiariosCampo
                .Include(d => d.DiarioEixos)
                .FirstOrDefaultAsync(d => d.IdDCampo == id);

            if (diarioCampo == null) return NotFound();

            await PreencherViewBagsDoBanco();
            return View(diarioCampo);
        }

        // ==========================================================
        // EDIT (POST)
        // ==========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiarioCampo diarioCampo, int[] EixosSelecionados)
        {
            if (HttpContext.Session.GetString("Email") == null) return RedirectToAction("Index", "Account");

            var usuarioLogado = await GetUsuarioLogadoAsync();
            if (usuarioLogado == null || !PermiteModulo(usuarioLogado, "DiariosCampo", "Atualizar"))
                return RedirectToAction(nameof(Index));

            if (id != diarioCampo.IdDCampo) return NotFound();

            // Remove validações de navegação
            ModelState.Remove("Comunidade");
            ModelState.Remove("Usuario");

            if (ModelState.IsValid)
            {
                try
                {
                    var diarioDb = await _context.DiariosCampo.FindAsync(id);
                    if (diarioDb == null) return NotFound();

                    // Atualiza campos
                    diarioDb.FkIdComunidade = diarioCampo.FkIdComunidade;
                    diarioDb.Data = diarioCampo.Data;
                    diarioDb.Descricao = diarioCampo.Descricao;
                    diarioDb.Localizacao = diarioCampo.Localizacao;
                    diarioDb.DtModificacao = DateTime.Now;

                    _context.Update(diarioDb);
                    await _context.SaveChangesAsync();

                    // Atualizar Eixos (Remove antigos e adiciona novos)
                    var eixosAntigos = _context.DiarioEixos.Where(de => de.FkIdDiario == id);
                    _context.DiarioEixos.RemoveRange(eixosAntigos);
                    
                    if (EixosSelecionados != null)
                    {
                        foreach (var eixoId in EixosSelecionados)
                        {
                            _context.DiarioEixos.Add(new DiarioEixo { FkIdDiario = id, FkIdEixo = eixoId });
                        }
                    }
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DiarioCampoExists(diarioCampo.IdDCampo)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PreencherViewBagsDoBanco();
            return View(diarioCampo);
        }

        // ==========================================================
        // DETAILS
        // ==========================================================
        public async Task<IActionResult> Details(int? id)
        {
            if (HttpContext.Session.GetString("Email") == null) return RedirectToAction("Index", "Account");
            if (id == null) return NotFound();

            var usuarioLogado = await GetUsuarioLogadoAsync();
            if (usuarioLogado == null || !PermiteModulo(usuarioLogado, "DiariosCampo", "Detalhar"))
                return RedirectToAction(nameof(Index));

            var diarioCampo = await _context.DiariosCampo
                .Include(d => d.Comunidade)
                .Include(d => d.DiarioEixos).ThenInclude(de => de.Eixo)
                .FirstOrDefaultAsync(m => m.IdDCampo == id);

            if (diarioCampo == null) return NotFound();

            return View(diarioCampo);
        }

        // ==========================================================
        // DELETE
        // ==========================================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (HttpContext.Session.GetString("Email") == null) return RedirectToAction("Index", "Account");
            if (id == null) return NotFound();

            var usuarioLogado = await GetUsuarioLogadoAsync();
            if (usuarioLogado == null || !PermiteModulo(usuarioLogado, "DiariosCampo", "Deletar"))
                return RedirectToAction("Index");

            var diarioCampo = await _context.DiariosCampo
                .Include(d => d.Comunidade)
                .FirstOrDefaultAsync(m => m.IdDCampo == id);

            if (diarioCampo == null) return NotFound();

            return View(diarioCampo);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");

            var usuarioLogado = await GetUsuarioLogadoAsync();
            if (usuarioLogado == null || !PermiteModulo(usuarioLogado, "DiariosCampo", "Deletar"))
                return RedirectToAction(nameof(Index));

            var diarioCampo = await _context.DiariosCampo.FindAsync(id);
            if (diarioCampo != null)
            {
                _context.DiariosCampo.Remove(diarioCampo);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================================
        // MÉTODOS AUXILIARES (PRIVADOS)
        // ==========================================================

        private bool DiarioCampoExists(int id)
        {
            return _context.DiariosCampo.Any(e => e.IdDCampo == id);
        }

        // Método para preencher ViewBags com DADOS REAIS do Banco
        private async Task PreencherViewBagsDoBanco()
        {
            // 1. Comunidades
            ViewBag.Comunidades = new SelectList(await _context.Comunidades.OrderBy(c => c.Nome).ToListAsync(), "Id_Comunidade", "Nome");

            // 2. Eixos (Para o select múltiplo)
            ViewBag.EixosList = await _context.Eixos.OrderBy(e => e.Nome).ToListAsync();

            // 3. Atores ativos (para menções e para o modal de ações).
            // Mantemos os dois formatos de propriedades para compatibilidade com
            // as views Razor (Id/Nome) e scripts existentes (Value/Text).
            var atores = await _context.Atores
                .Where(a => a.Ativo == "S")
                .OrderBy(a => a.Nome)
                .Select(a => new
                {
                    Id = a.IdAtores,
                    Nome = a.Nome,
                    Value = a.IdAtores,
                    Text = a.Nome
                })
                .ToListAsync();

            ViewBag.Atores = atores;
            ViewBag.AtoresList = atores;

            // 4. Atividades (Para o Modal de Ação da Equipe)
            ViewBag.Atividades = await _context.Atividades.OrderBy(a => a.Nome).ToListAsync();
        }

        // Auxiliar para pegar usuário e permissões
        private async Task<Usuario?> GetUsuarioLogadoAsync()
        {
            var idString = HttpContext.Session.GetString("ID");
            if (string.IsNullOrEmpty(idString)) return null;

            return await _context.Usuarios
                .Include(u => u.Perfil)
                .ThenInclude(p => p.Permissoes)
                .FirstOrDefaultAsync(u => u.IdUsuario == int.Parse(idString));
        }

        // Auxiliar para verificar permissão
        private static bool PermiteModulo(Usuario usuario, string modulo, string acao) => acao switch
        {
            "Listar" => usuario.CanList(modulo),
            "Criar" => usuario.CanCreate(modulo),
            "Atualizar" => usuario.CanUpdate(modulo),
            "Detalhar" => usuario.CanViewDetails(modulo),
            "Deletar" => usuario.CanDelete(modulo),
            _ => false
        };
    }
}
