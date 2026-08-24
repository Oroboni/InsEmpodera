using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Empodera.Models;
using Empodera.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Empodera.Controllers
{
    public class RelatoriosController : BaseController
    {

        private readonly ILogger<AtoresController> _logger;
        private readonly ApplicationDbContext _context;

        public RelatoriosController(ILogger<AtoresController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Email") == null) 
            { 
                return RedirectToAction("Index", "Account"); 
            }
            return View();
        }

        public async Task<IActionResult> Rsc()
        {
            if (HttpContext.Session.GetString("Email") == null) 
            { 
                return RedirectToAction("Index", "Account"); 
            }

            ViewBag.Comunidades = new SelectList(
                await _context.Comunidades.ToListAsync(),
                "Id_Comunidade",
                "Nome"
            );

            return View();
        }

        public async Task<IActionResult> PrimaryNetwork()
        {
            if (HttpContext.Session.GetString("Email") == null) 
            { 
                return RedirectToAction("Index", "Account"); 
            }

            ViewBag.Comunidades = new SelectList(
                await _context.Comunidades.ToListAsync(),
                "Id_Comunidade",
                "Nome"
            );

            return View();
        }

        public async Task<IActionResult> PersonalAssessment()
        {
            if (HttpContext.Session.GetString("Email") == null) 
            { 
                return RedirectToAction("Index", "Account"); 
            }
            return View();
        }

        public async Task<IActionResult> FirstContact()
        {
            if (HttpContext.Session.GetString("Email") == null) 
            { 
                return RedirectToAction("Index", "Account"); 
            }

            ViewBag.Comunidades = new SelectList(
                await _context.Comunidades.ToListAsync(),
                "Id_Comunidade",
                "Nome"
            );
            
            return View();
        }

        public async Task<IActionResult> Actions()
        {
            if (HttpContext.Session.GetString("Email") == null) 
            { 
                return RedirectToAction("Index", "Account"); 
            }

            ViewBag.Comunidades = new SelectList(
                await _context.Comunidades.ToListAsync(),
                "Id_Comunidade",
                "Nome"
            );

            return View();
        }
    }
}