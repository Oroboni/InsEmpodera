using Microsoft.AspNetCore.Mvc;

namespace InsEmpodera.Controllers
{
    public class DiarioProcessoPessoalController : BaseController
    {
        public IActionResult Index()
        {
            return DefaultIndex();
        }
    }
}