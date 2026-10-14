using Microsoft.AspNetCore.Mvc;

namespace Empodera.Controllers
{
    public class DashboardController : BaseController
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Email") == null) 
            { 
                return RedirectToAction("Index", "Account"); 
            }
            // Rota antiga sem página própria: a tela equivalente fica em HomeController.
            return RedirectToAction("Index", "Home");
        }
    }
}