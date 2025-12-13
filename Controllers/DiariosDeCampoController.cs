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
            return DefaultIndex();
        }
    }
}