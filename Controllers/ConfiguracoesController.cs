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
            if (db is null || !int.TryParse(HttpContext.Session.GetString("ID"), out var usuarioId))
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
            ViewBag.CoresPersonalizadas = int.TryParse(HttpContext.Session.GetString("ID"), out var usuarioId)
                && HttpContext.RequestServices.GetService<TemaPersonalizadoService>()?.Obter(usuarioId) is { Count: > 0 };
            return DefaultIndex();
        }

        // POST: /Configuracoes/SalvarCores — cores da plataforma do próprio usuário logado.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SalvarCores(Dictionary<string, string> cores)
        {
            if (HttpContext.Session.GetString("Email") == null || !int.TryParse(HttpContext.Session.GetString("ID"), out var usuarioId))
                return RedirectToAction("Index", "Account");

            try
            {
                HttpContext.RequestServices.GetRequiredService<TemaPersonalizadoService>().Salvar(usuarioId, cores ?? new());
                TempData["CoresMensagem"] = "Suas cores foram salvas. Elas valem só para a sua conta.";
            }
            catch (ArgumentException ex)
            {
                TempData["CoresErro"] = ex.Message;
            }
            return Redirect(Url.Action(nameof(Index)) + "#cores");
        }

        // POST: /Configuracoes/RestaurarCores — o usuário logado volta às cores padrão.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RestaurarCores()
        {
            if (HttpContext.Session.GetString("Email") == null || !int.TryParse(HttpContext.Session.GetString("ID"), out var usuarioId))
                return RedirectToAction("Index", "Account");

            HttpContext.RequestServices.GetRequiredService<TemaPersonalizadoService>().Restaurar(usuarioId);
            TempData["CoresMensagem"] = "Cores padrão restauradas.";
            return Redirect(Url.Action(nameof(Index)) + "#cores");
        }
    }
}