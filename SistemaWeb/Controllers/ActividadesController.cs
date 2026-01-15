using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Models;
using SistemaWeb.Services;



namespace SistemaWeb.Controllers
{
    public class ActividadesController : Controller
    {
        // Change Repository to Service
        private readonly ActividadService _service;

        public ActividadesController(ActividadService service)
        {
            _service = service;
        }

        public IActionResult Index()
        {
            // Call the service
            var actividades = _service.ObtenerTodas();
            return View(actividades);
        }

        [HttpPost]
        public IActionResult Crear(Actividad actividad)
        {
            if (ModelState.IsValid)
            {
                // The Service handles the 'Cupo' logic now
                _service.Agregar(actividad);
                return RedirectToAction(nameof(Index));
            }
            return View(actividad);
        }
    }
}