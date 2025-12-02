using Microsoft.AspNetCore.Mvc;

namespace Empodera.Controllers
{
    public class DashboardController : BaseController
    {
        public IActionResult Index()
        {
            return DefaultIndex();
        }
    }
}