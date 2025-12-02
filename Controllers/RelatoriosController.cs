using Microsoft.AspNetCore.Mvc;

namespace Empodera.Controllers
{
    public class RelatoriosController : BaseController
    {
        public IActionResult Index()
        {
            return DefaultIndex();
        }
    }
}