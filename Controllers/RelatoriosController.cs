using Microsoft.AspNetCore.Mvc;

namespace InsEmpodera.Controllers
{
    public class RelatoriosController : BaseController
    {
        public IActionResult Index()
        {
            return DefaultIndex();
        }
    }
}