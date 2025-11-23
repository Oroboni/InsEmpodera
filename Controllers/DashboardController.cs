using Microsoft.AspNetCore.Mvc;

namespace InsEmpodera.Controllers
{
    public class DashboardController : BaseController
    {
        public IActionResult Index()
        {
            return DefaultIndex();
        }
    }
}