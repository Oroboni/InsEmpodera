using Microsoft.AspNetCore.Mvc;

namespace InsEmpodera.Controllers
{
    public class ConfiguracoesController : BaseController
    {
        public IActionResult Index()
        {
            return DefaultIndex();
        }
    }
}