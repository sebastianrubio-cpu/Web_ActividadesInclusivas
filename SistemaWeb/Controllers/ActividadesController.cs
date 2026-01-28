using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Models;
using SistemaWeb.Services;
using System.Threading.Tasks; // Necesario para Task

namespace SistemaWeb.Controllers
{
    public class ActividadesController : Controller
    {
        private readonly ActividadService _service;
        private readonly EstudiantesClient _apiClient; // [Nuevo] Inyeccion para el Ping

        // Actualizamos el constructor para recibir el cliente de la API
        public ActividadesController(ActividadService service, EstudiantesClient apiClient)
        {
            _service = service;
            _apiClient = apiClient;
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

        // 6. ELIMINAR
        [HttpPost]
        public IActionResult Eliminar(string codigo)
        {
            _service.Eliminar(codigo);
            return RedirectToAction(nameof(Index));
        }

        // 7. VERIFICAR SISTEMA (PING)
       
        public async Task<IActionResult> VerificarSistema()
        {
            
            var reporte = await ValidarSistema.RealizarChequeo(_apiClient);
            

            return View(reporte); 
        }
    }
}