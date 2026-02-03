using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Models;
using SistemaWeb.Services;
using System.Linq;
using System.Threading.Tasks;

namespace SistemaWeb.Controllers
{
    [Authorize]
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

        // --- ZONA PÚBLICA / ESTUDIANTE ---

        // 1. LISTADO (Tabla clásica)
        [HttpGet]
        public IActionResult Index()
        {
            var actividades = _service.ObtenerTodas();
            return View(actividades);
        }

        // 2. NUEVA VISTA MINIMALISTA (Para Estudiantes)
        [Authorize(Roles = "Estudiante,Administrador,Profesor")]
        [HttpGet]
        public IActionResult VistaEstudiante()
        {
            var actividades = _service.ObtenerTodas();
            // Opcional: Filtrar solo activas
            // var activas = actividades.Where(a => a.Estado == "Activo").ToList();
            return View(actividades);
        }

        // 3. VERIFICAR SISTEMA
        [HttpGet]
        public async Task<IActionResult> VerificarSistema()
        {
            var reporte = await ValidarSistema.RealizarChequeo(_apiClient);
            return View(reporte);
        }

        // 4. MAPA
        [HttpGet]
        public IActionResult Mapa()
        {
            var actividades = _service.ObtenerTodas();
            return View(actividades);
        }

        // --- ZONA PROTEGIDA (Administrador y Profesor) ---

        [Authorize(Roles = "Administrador,Profesor")]
        [HttpGet]
        public IActionResult Crear()
        {
            return View();
        }

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

        [Authorize(Roles = "Administrador,Profesor")]
        [HttpGet]
        public IActionResult Editar(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var actividad = _service.ObtenerPorId(id);
            if (actividad == null) return NotFound();
            return View(actividad);
        }

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

        // 5. ESTADÍSTICAS INTERACTIVAS
        [Authorize(Roles = "Administrador,Profesor")]
        public IActionResult Estadisticas()
        {
            var actividades = _service.ObtenerTodas();

            // Preparar datos para gráficos
            var agrupado = actividades
                .GroupBy(a => a.TipoDiscapacidad)
                .Select(g => new {
                    Label = string.IsNullOrEmpty(g.Key) ? "General" : g.Key,
                    Cantidad = g.Count(),
                    CupoTotal = g.Sum(x => x.Cupo)
                })
                .ToList();

            // Arrays para Chart.js
            ViewBag.Labels = agrupado.Select(x => x.Label).ToArray();
            ViewBag.DataCantidad = agrupado.Select(x => x.Cantidad).ToArray();
            ViewBag.DataCupos = agrupado.Select(x => x.CupoTotal).ToArray();

            // Tarjetas KPI
            ViewBag.TotalActividades = actividades.Count;
            ViewBag.TotalCupos = actividades.Sum(a => a.Cupo);
            ViewBag.UsuarioTotal = _usuarioRepo.ContarUsuarios();

            return View();
        }
    }
}