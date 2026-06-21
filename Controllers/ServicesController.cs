using Microsoft.AspNetCore.Mvc;
using Empodera.Data;
using Empodera.Services;
using Microsoft.EntityFrameworkCore;

namespace Empodera.Controllers
{
    public class ServicesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public ServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult ExportComunidade(int id)
        {
            var comunidade = _context.Comunidades
                .Include(c => c.RedeRecursos)
                .Include(c => c.Atividades)
                .FirstOrDefault(c => c.Id_Comunidade == id);

            var atores = _context.AtorComunidades
                .Where(ac => ac.FkIdComunidade == id)
                .Include(ac => ac.Ator)
                    .ThenInclude(a => a.Redes)
                .Include(ac => ac.Ator)
                    .ThenInclude(a => a.Avaliacoes)
                .Select(ac => ac.Ator)
                .ToList();

            var exportService = new ExportComunidade();

            var arquivoExcel = exportService
                .GerarRelatorioComunidade(new[] { comunidade }, comunidade, atores);

            return File(
                arquivoExcel,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{comunidade?.Nome}.xlsx"
            );
        }
    }
}