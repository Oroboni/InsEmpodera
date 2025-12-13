using Microsoft.AspNetCore.Mvc;

namespace Empodera.Controllers
{
    public class DiarioProcessoPessoalController : BaseController
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