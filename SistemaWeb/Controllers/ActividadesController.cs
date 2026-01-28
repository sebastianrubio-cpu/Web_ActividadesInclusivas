using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Models;
using SistemaWeb.Services;

namespace SistemaWeb.Controllers
{
    public class ActividadesController : Controller
    {
        // Usamos el Servicio en lugar del Repositorio directo
        private readonly ActividadService _service;

        public ActividadesController(ActividadService service)
        {
            _service = service;
        }

        // 1. LISTADO (GET)
        public IActionResult Index()
        {
            var actividades = _service.ObtenerTodas();
            return View(actividades);
        }

        // 2. CREAR (GET)
        public IActionResult Crear()
        {
            return View();
        }

        // 3. CREAR - GUARDAR DATOS (POST)
        [HttpPost]
        public IActionResult Crear(Actividad actividad)
        {
            if (ModelState.IsValid)
            {
                _service.Agregar(actividad);
                return RedirectToAction(nameof(Index));
            }
            return View(actividad);
        }

        // 4. EDITAR (GET)
        public IActionResult Editar(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound();
            }

            var actividad = _service.ObtenerPorId(id);
            if (actividad == null)
            {
                return NotFound();
            }
            return View(actividad);
        }

        // 5. EDITAR - GUARDAR CAMBIOS (POST)
        [HttpPost]
        public IActionResult Editar(Actividad actividad)
        {
            if (ModelState.IsValid)
            {
                _service.Actualizar(actividad);
                return RedirectToAction(nameof(Index));
            }
            return View(actividad);
        }



        [HttpPost]
        public IActionResult Eliminar(string codigo)
        {
            _repository.Eliminar(codigo);

            
            _service.Eliminar(codigo);

            return RedirectToAction(nameof(Index));
        }
    }
}