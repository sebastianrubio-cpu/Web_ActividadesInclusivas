using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Models;
using SistemaWeb.Services;
using System.Diagnostics;

namespace SistemaWeb.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly EstudiantesClient _estudiantesClient;

        public HomeController(ILogger<HomeController> logger, EstudiantesClient estudiantesClient)
        {
            _logger = logger;
            _estudiantesClient = estudiantesClient;
        }

        public async Task<IActionResult> Index()
        {
            var estado = await ValidarSistema.RealizarChequeo(_estudiantesClient);
            return View(estado);
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