using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Models;

namespace SistemaWeb.Controllers
{
    public class ActividadesController : Controller
    {
        private readonly ActividadRepository _repository;

        public ActividadesController(ActividadRepository repository)
        {
            _repository = repository;
        }

        public IActionResult Index()
        {
            var actividades = _repository.ObtenerTodas();
            return View(actividades);
        }

        public IActionResult Crear()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Actividad actividad)
        {
            if (ModelState.IsValid)
            {
                _repository.Agregar(actividad);
                return RedirectToAction(nameof(Index));
            }
            return View(actividad);
        }

        // CORRECCIÓN: Ahora recibimos un string id (Codigo), no un int
        public IActionResult Editar(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var actividad = _repository.ObtenerPorId(id);
            if (actividad == null)
            {
                return NotFound();
            }
            return View(actividad);
        }

        [HttpPost]
        public IActionResult Editar(Actividad actividad)
        {
            if (ModelState.IsValid)
            {
                _repository.Actualizar(actividad);
                return RedirectToAction(nameof(Index));
            }
            return View(actividad);
        }
    }
}