using Microsoft.AspNetCore.Mvc;

namespace Empodera.Controllers
{
    public class UsuariosController : BaseController
    {
        public IActionResult Index()
        {
            return DefaultIndex();
        }
    }
}