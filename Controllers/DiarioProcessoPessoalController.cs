using Microsoft.AspNetCore.Mvc;

namespace Empodera.Controllers
{
    public class DiarioProcessoPessoalController : BaseController
    {
        public IActionResult Index()
        {
            return DefaultIndex();
        }
    }
}