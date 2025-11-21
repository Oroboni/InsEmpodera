using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Empodera.Data;
using Empodera.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Empodera.Controllers
{
    public class DiarioCampoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public DiarioCampoController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // GET: /DiarioCampo
        public async Task<IActionResult> Index(string search)
        {
            var q = _context.DiariosCampo
                .Include(d => d.Comunidade)
                .Include(d => d.Eixos)
                .OrderByDescending(d => d.DataCriacao)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                q = q.Where(d =>
                    d.Descricao.ToLower().Contains(s) ||
                    d.Comunidade.Nome.ToLower().Contains(s));
            }

            var list = await q.ToListAsync();
            return View(list);
        }

        // GET: /DiarioCampo/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var diario = await _context.DiariosCampo
                .Include(d => d.Comunidade)
                .Include(d => d.Eixos).ThenInclude(de => de.Eixo)
                .Include(d => d.DiarioAcoes).ThenInclude(da => da.Acoes).ThenInclude(a => a.AcoesAtores)
                .Include(d => d.Anexos)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (diario == null) return NotFound();

            ViewBag.Atores = _context.Atores.OrderBy(a => a.Nome).ToList();
            ViewBag.Eixos = _context.Eixos.OrderBy(e => e.Nome).ToList();
            return View(diario);
        }

        // GET: /DiarioCampo/Create
        public IActionResult Create()
        {
            CarregarSelects();
            ViewData["PageCSS"] = "diariocampoform";
            return View();
        }

        // POST: /DiarioCampo/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DiarioCampo model, IFormFile[] AnexosDiario)
        {
            if (!ModelState.IsValid)
            {
                CarregarSelects();
                ViewData["PageCSS"] = "diariocampoform";
                return View(model);
            }

            model.DataCriacao = DateTime.Now;
            model.UltimaAtualizacao = DateTime.Now;
            // preencher CriadoPor se houver sistema de usuário:
            // model.CriadoPor = User.Identity.Name ?? "system";

            _context.DiariosCampo.Add(model);
            await _context.SaveChangesAsync();

            // salvar eixos (se vierem via campo EixosSelected como lista de ids)
            if (Request.Form.ContainsKey("EixosSelected"))
            {
                var selected = Request.Form["EixosSelected"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in selected)
                {
                    if (int.TryParse(s, out var eixoId))
                    {
                        _context.DiarioEixos.Add(new DiarioEixo { DiarioId = model.Id, EixoId = eixoId });
                    }
                }
                await _context.SaveChangesAsync();
            }

            // salvar AnexosDiario
            if (AnexosDiario != null && AnexosDiario.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/diarios");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                foreach (var f in AnexosDiario)
                {
                    if (f.Length == 0) continue;
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(f.FileName)}";
                    var full = Path.Combine(uploads, fileName);
                    using (var fs = new FileStream(full, FileMode.Create))
                    {
                        await f.CopyToAsync(fs);
                    }
                    _context.AnexosDiario.Add(new AnexosDiario
                    {
                        DiarioId = model.Id,
                        Caminho = $"/uploads/diarios/{fileName}"
                    });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: /DiarioCampo/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var diario = await _context.DiariosCampo
                .Include(d => d.Eixos)
                .Include(d => d.Anexos)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (diario == null) return NotFound();
            CarregarSelects();
            ViewData["PageCSS"] = "diariocampoform";
            return View(diario);
        }

        // POST: /DiarioCampo/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DiarioCampo model, IFormFile[] AnexosDiario)
        {
            if (id != model.Id) return BadRequest();

            if (!ModelState.IsValid)
            {
                CarregarSelects();
                ViewData["PageCSS"] = "diariocampoform";
                return View(model);
            }

            var diarioDb = await _context.DiariosCampo
                .Include(d => d.Eixos)
                .Include(d => d.Anexos)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (diarioDb == null) return NotFound();

            // Atualizar campos básicos
            diarioDb.ComunidadeId = model.ComunidadeId;
            diarioDb.DataCriacao = model.DataCriacao;
            diarioDb.Descricao = model.Descricao;
            diarioDb.CEP = model.CEP;
            diarioDb.UltimaAtualizacao = DateTime.Now;
            diarioDb.CriadoPor = model.CriadoPor; // se desejar manter/atualizar

            _context.Update(diarioDb);
            await _context.SaveChangesAsync();

            // Atualizar eixos: simplificação — remover existentes e inserir novos (pode otimizar)
            var existEixos = _context.DiarioEixos.Where(de => de.DiarioId == id);
            _context.DiarioEixos.RemoveRange(existEixos);
            if (Request.Form.ContainsKey("EixosSelected"))
            {
                var selected = Request.Form["EixosSelected"].ToString().Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var s in selected)
                {
                    if (int.TryParse(s, out var eixoId))
                        _context.DiarioEixos.Add(new DiarioEixo { DiarioId = id, EixoId = eixoId });
                }
            }
            await _context.SaveChangesAsync();

            // AnexosDiario novos
            if (AnexosDiario != null && AnexosDiario.Length > 0)
            {
                var uploads = Path.Combine(_env.WebRootPath, "uploads/diarios");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                foreach (var f in AnexosDiario)
                {
                    if (f.Length == 0) continue;
                    var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(f.FileName)}";
                    var full = Path.Combine(uploads, fileName);
                    using (var fs = new FileStream(full, FileMode.Create))
                    {
                        await f.CopyToAsync(fs);
                    }
                    _context.AnexosDiario.Add(new AnexosDiario
                    {
                        DiarioId = id,
                        Caminho = $"/uploads/diarios/{fileName}"
                    });
                }
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }

        // GET: /DiarioCampo/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            var diario = await _context.DiariosCampo
                .Include(d => d.Comunidade)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (diario == null) return NotFound();
            return View(diario);
        }

        // POST: /DiarioCampo/DeleteConfirmed/5
        [HttpPost, ActionName("DeleteConfirmed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var diario = await _context.DiariosCampo.FindAsync(id);
            if (diario != null)
            {
                // remover AnexosDiario fisicos e registros
                var AnexosDiario = _context.AnexosDiario.Where(a => a.DiarioId == id).ToList();
                foreach (var a in AnexosDiario)
                {
                    var fullPath = Path.Combine(_env.WebRootPath, a.Caminho.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                    if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
                }
                _context.AnexosDiario.RemoveRange(AnexosDiario);

                // remover eixos, acoes relacionadas
                _context.DiarioEixos.RemoveRange(_context.DiarioEixos.Where(de => de.DiarioId == id));
                _context.DiarioAcoes.RemoveRange(_context.DiarioAcoes.Where(da => da.DiarioId == id));

                _context.DiariosCampo.Remove(diario);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // POST: /DiarioCampo/UploadAnexo
        [HttpPost]
        public async Task<IActionResult> UploadAnexo(int diarioId, IFormFile file)
        {
            if (file == null || diarioId == 0) return BadRequest();

            var uploads = Path.Combine(_env.WebRootPath, "uploads/diarios");
            if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

            var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(file.FileName)}";
            var full = Path.Combine(uploads, fileName);
            using (var fs = new FileStream(full, FileMode.Create))
            {
                await file.CopyToAsync(fs);
            }

            var anexo = new AnexosDiario { DiarioId = diarioId, Caminho = $"/uploads/diarios/{fileName}" };
            _context.AnexosDiario.Add(anexo);
            await _context.SaveChangesAsync();

            return Json(new { success = true, path = anexo.Caminho, id = anexo.IdAnexos });
        }

        // POST: /DiarioCampo/RemoverAnexo
        [HttpPost]
        public async Task<IActionResult> RemoverAnexo(int id)
        {
            var anexo = await _context.AnexosDiario.FindAsync(id);
            if (anexo == null) return NotFound();

            var fullPath = Path.Combine(_env.WebRootPath, anexo.Caminho.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);

            _context.AnexosDiario.Remove(anexo);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        // GET: /DiarioCampo/Autocomplete?term=...
        [HttpGet]
        public IActionResult Autocomplete(string q, string type = "ator")
        {
            // type: ator | comunidade | atividade
            if (string.IsNullOrWhiteSpace(q)) return Json(Array.Empty<object>());

            q = q.ToLower();
            if (type == "ator")
            {
                var items = _context.Atores.Where(a => a.Nome.ToLower().Contains(q))
                    .Select(a => new { id = a.IdAtores, label = a.Nome })
                    .Take(10).ToList();
                return Json(items);
            }
            if (type == "comunidade")
            {
                var items = _context.Comunidades.Where(c => c.Nome.ToLower().Contains(q))
                    .Select(c => new { id = c.IdComunidade, label = c.Nome })
                    .Take(10).ToList();
                return Json(items);
            }
            // atividade
            var acts = _context.Atividades.Where(at => at.Nome.ToLower().Contains(q))
                .Select(at => new { id = at.IdAtividade, label = at.Nome })
                .Take(10).ToList();
            return Json(acts);
        }

        // GET: /DiarioCampo/BuscarCep?cep=01001000
        [HttpGet]
        public async Task<IActionResult> BuscarCep(string cep)
        {
            if (string.IsNullOrWhiteSpace(cep)) return BadRequest();
            // implementação simples utilizando ViaCEP pública
            try
            {
                var normalized = new string(cep.Where(char.IsDigit).ToArray());
                var url = $"https://viacep.com.br/ws/{normalized}/json/";
                using var client = new System.Net.Http.HttpClient();
                var resp = await client.GetStringAsync(url);
                return Content(resp, "application/json");
            }
            catch
            {
                return StatusCode(500);
            }
        }

        private void CarregarSelects()
        {
            ViewBag.Comunidades = _context.Comunidades.OrderBy(c => c.Nome).Select(c => new { c.IdComunidade, c.Nome }).ToList();
            ViewBag.Atores = _context.Atores.OrderBy(a => a.Nome).Select(a => new { a.IdAtores, a.Nome }).ToList();
            ViewBag.Atividades = _context.Atividades.OrderBy(at => at.Nome).Select(at => new { at.IdAtividade, at.Nome }).ToList();
            ViewBag.Eixos = _context.Eixos.OrderBy(e => e.Nome).Select(e => new { e.IdEixo, e.Nome }).ToList();
        }
    }
}
