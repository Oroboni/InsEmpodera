using Microsoft.AspNetCore.Mvc;

namespace InsEmpodera.Controllers
{
    public class DiariosDeCampoController : BaseController
    {
        public IActionResult Index()
        {
            return DefaultIndex();
        }
    }
}