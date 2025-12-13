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
            return DefaultIndex();
        }
    }
}