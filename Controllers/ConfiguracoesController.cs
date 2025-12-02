using Microsoft.AspNetCore.Mvc;

namespace Empodera.Controllers
{
    public class ConfiguracoesController : BaseController
    {
        public IActionResult Index()
        {
            return DefaultIndex();
        }
    }
}