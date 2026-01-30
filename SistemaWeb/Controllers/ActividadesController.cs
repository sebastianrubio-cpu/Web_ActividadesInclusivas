using Microsoft.AspNetCore.Authorization; // Necesario para [Authorize]
using Microsoft.AspNetCore.Mvc;           // Necesario para Controller, IActionResult
using SistemaWeb.Models;
using SistemaWeb.Services;
using System.Threading.Tasks;             // Necesario para async/await

namespace SistemaWeb.Controllers
{
    [Authorize] // 1. CANDADO GENERAL: Nadie entra al controlador si no está logueado
    public class ActividadesController : Controller
    {
        private readonly ActividadService _service;
        private readonly EstudiantesClient _apiClient;
        private readonly UsuarioRepository _usuarioRepo;

        public ActividadesController(ActividadService service, EstudiantesClient apiClient, UsuarioRepository usuarioRepo)
        {
            _service = service;
            _apiClient = apiClient;
            _usuarioRepo = usuarioRepo;

        }

        // --- ZONA PÚBLICA (Accesible para Estudiantes, Profesores y Admins) ---

        // 1. LISTADO
        [HttpGet]
        public IActionResult Index()
        {
            var actividades = _service.ObtenerTodas();
            return View(actividades);
        }

        // 2. VERIFICAR SISTEMA (PING)
        [HttpGet]
        public async Task<IActionResult> VerificarSistema()
        {
            var reporte = await ValidarSistema.RealizarChequeo(_apiClient);
            return View(reporte);
        }

        // 3. VISTA DE MAPA 
        [HttpGet]
        public IActionResult Mapa()
        {
            var actividades = _service.ObtenerTodas();
            return View(actividades);
        }

        // --- ZONA PROTEGIDA (Solo para Administrativos y Profesores) ---
        // Si un Estudiante intenta entrar aquí, recibirá un error 403 o será redirigido.

        // 4. CREAR (GET)
        [Authorize(Roles = "Administrador,Profesor")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

        // 5. CREAR (POST)
        [Authorize(Roles = "Administrador,Profesor")]
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

        // 6. EDITAR (GET) Cambiar
        [Authorize(Roles = "Administrador,Profesor")]
        [HttpGet]
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

        // 7. EDITAR (POST) Pedir SQL
        [Authorize(Roles = "Administrador,Profesor")]
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

        // 8. ELIMINAR
        
        [Authorize(Roles = "Administrador,Profesor")]
        [HttpPost] 
        public IActionResult Eliminar(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _service.Eliminar(id);
            }
            return RedirectToAction(nameof(Index));
        }


        // 9. ESTADÍSTICAS (Corregido)
        [Authorize(Roles = "Administrador,Profesor")]
        public IActionResult Estadisticas()
        {
            ViewBag.TotalUsuarios = _usuarioRepo.ContarUsuarios();
            var actividades = _service.ObtenerTodas();
            ViewBag.TotalActividades = actividades.Count;

            // Agrupamos y sacamos los nombres y conteos reales
            var resumen = actividades
                .GroupBy(a => a.TipoDiscapacidad)
                .Select(g => new {
                    Nombre = string.IsNullOrEmpty(g.Key) ? "No asignada" : g.Key,
                    Total = g.Count()
                }).ToList();

            // Pasamos las listas por separado para el gráfico
            ViewBag.LabelsDiscapacidad = resumen.Select(r => r.Nombre).ToArray();
            ViewBag.ValoresDiscapacidad = resumen.Select(r => r.Total).ToArray();

            return View();
        }
    }
}