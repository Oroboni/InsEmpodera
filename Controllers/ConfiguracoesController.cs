using Empodera.Data;
using Empodera.Models;
using Empodera.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Empodera.Controllers
{
    public class ConfiguracoesController : BaseController
    {
        // Resumo da conta logada (nome, e-mail, perfil) para a seção "Sua conta".
        private async Task<ContaUsuarioResumo?> CarregarContaAsync()
        {
            var db = HttpContext.RequestServices.GetService<ApplicationDbContext>();
            if (db is null || !Guid.TryParse(HttpContext.Session.GetString("ID"), out var usuarioId))
                return null;

            var usuario = await db.Usuarios.AsNoTracking()
                .Include(u => u.Perfil).ThenInclude(p => p.Permissoes)
                .FirstOrDefaultAsync(u => u.IdUsuario == usuarioId);
            if (usuario is null)
                return null;

            return new ContaUsuarioResumo(usuario.Nome, usuario.Email, usuario.Perfil?.Nome ?? "—",
                usuario.Ocupacao, usuario.CanList("Perfis"),
                string.Equals(usuario.Perfil?.Nome, "Admin", StringComparison.OrdinalIgnoreCase));
        }

        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Email") == null) 
            { 
                return RedirectToAction("Index", "Account"); 
            }
            ViewBag.Conta = await CarregarContaAsync();
            ViewBag.TemaAtivo = (HttpContext.RequestServices.GetService<IConfiguration>()?["Tema"] ?? "verde").Trim().ToLowerInvariant();
            ViewBag.CoresPersonalizadas = HttpContext.RequestServices.GetService<TemaPersonalizadoService>()?.Obter() is { Count: > 0 };
            return DefaultIndex();
        }

        // POST: /Configuracoes/SalvarCores — somente administradores.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SalvarCores(Dictionary<string, string> cores)
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");
            var conta = await CarregarContaAsync();
            if (conta?.IsAdmin != true)
                return StatusCode(StatusCodes.Status403Forbidden);

            try
            {
                HttpContext.RequestServices.GetRequiredService<TemaPersonalizadoService>().Salvar(cores ?? new());
                TempData["CoresMensagem"] = "Cores salvas. Elas já valem para todos os usuários.";
            }
            catch (ArgumentException ex)
            {
                TempData["CoresErro"] = ex.Message;
            }
            return Redirect(Url.Action(nameof(Index)) + "#cores");
        }

        // POST: /Configuracoes/RestaurarCores — volta ao tema padrão. Somente administradores.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestaurarCores()
        {
            if (HttpContext.Session.GetString("Email") == null)
                return RedirectToAction("Index", "Account");
            var conta = await CarregarContaAsync();
            if (conta?.IsAdmin != true)
                return StatusCode(StatusCodes.Status403Forbidden);

            HttpContext.RequestServices.GetRequiredService<TemaPersonalizadoService>().Restaurar();
            TempData["CoresMensagem"] = "Cores padrão restauradas.";
            return Redirect(Url.Action(nameof(Index)) + "#cores");
        }
    }
}