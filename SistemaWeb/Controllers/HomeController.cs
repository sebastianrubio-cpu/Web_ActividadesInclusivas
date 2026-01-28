using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Authorization; // Importante para permitir acceso

namespace SistemaWeb.Controllers
{
    // [Authorize] // Descomenta si quieres que solo logueados vean esto
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            
            return RedirectToAction("Index", "Actividades");
        }

        public IActionResult Proyecto()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}