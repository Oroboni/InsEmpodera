using Microsoft.AspNetCore.Mvc;

namespace InsEmpodera.Controllers
{
    public abstract class BaseController : Controller
    {
        protected IActionResult DefaultIndex()
        {
            if (HttpContext.Session.GetString("Email") == null)
            {
                return RedirectToAction("Index", "Account");
            }

            // Configura o PageCSS automaticamente baseado no nome do Controller
            var controllerName = this.GetType().Name.Replace("Controller", "").ToLower();
            ViewData["PageCSS"] = controllerName;
            ViewData["Title"] = controllerName;

            // Se for uma requisição AJAX, retorna apenas o conteúdo parcial
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("Index");
            }

            return View("Index");
        }
    }
}