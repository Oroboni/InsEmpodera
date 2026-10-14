using Microsoft.AspNetCore.Mvc;

namespace Empodera.Controllers
{
    public class DiariosDeCampoController : BaseController
    {
        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Email") == null) 
            { 
                return RedirectToAction("Index", "Account"); 
            }
            // Rota antiga sem página própria: a tela equivalente fica em DiarioCampoController.
            return RedirectToAction("Index", "DiarioCampo");
        }
    }
}