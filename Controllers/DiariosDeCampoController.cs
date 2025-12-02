using Microsoft.AspNetCore.Mvc;

namespace Empodera.Controllers
{
    public class DiariosDeCampoController : BaseController
    {
        public IActionResult Index()
        {
            return DefaultIndex();
        }
    }
}