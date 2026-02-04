using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Models;
using SistemaWeb.Services;
using System.Linq;
using System.Security.Claims; // Necesario para obtener el ID del usuario
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

        // --- MÉTODOS DE LECTURA (Sin cambios mayores) ---

        [HttpGet]
        public IActionResult Index()
        {
            var actividades = _service.ObtenerTodas();
            return View(actividades);
        }

        [Authorize(Roles = "Estudiante,Administrador,Profesor")]
        [HttpGet]
        public IActionResult VistaEstudiante()
        {
            var actividades = _service.ObtenerTodas();
            return View(actividades);
        }

        [HttpGet]
        public async Task<IActionResult> VerificarSistema()
        {
            var reporte = await ValidarSistema.RealizarChequeo(_apiClient);
            return View(reporte);
        }

        [HttpGet]
        public IActionResult Mapa()
        {
            var actividades = _service.ObtenerTodas();
            return View(actividades);
        }

        // --- ZONA DE EDICIÓN (ADAPTADA A LA NUEVA DB) ---

        [Authorize(Roles = "Administrador,Profesor")]
        [HttpGet]
        public IActionResult Crear()
        {
            // Cargamos la lista de profesores para el Dropdown en la vista
            ViewBag.ListaProfesores = _usuarioRepo.ObtenerProfesores();
            return View();
        }

        [Authorize(Roles = "Administrador,Profesor")]
        [HttpPost]
        public IActionResult Crear(Actividad actividad)
        {
            // Obtenemos el ID del usuario actual para la Auditoría
            string idAuditoria = User.FindFirstValue("IdUsuario");

            // Validaciones básicas manuales si ModelState falla por campos opcionales
            if (!string.IsNullOrEmpty(actividad.Codigo) && !string.IsNullOrEmpty(actividad.Nombre) && !string.IsNullOrEmpty(actividad.IdResponsable))
            {
                _service.Agregar(actividad, idAuditoria);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ListaProfesores = _usuarioRepo.ObtenerProfesores();
            return View(actividad);
        }

        [Authorize(Roles = "Administrador,Profesor")]
        [HttpGet]
        public IActionResult Editar(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var actividad = _service.ObtenerPorId(id);
            if (actividad == null) return NotFound();

            // TRUCO DE ADAPTACIÓN:
            // El SP 'sp_ObtenerActividadPorId' NO devuelve el IdResponsable (devuelve el Nombre).
            // Para que el dropdown de Editar funcione y marque al profesor correcto,
            // buscamos al usuario por su correo (que sí viene del SP) para obtener su ID.
            if (!string.IsNullOrEmpty(actividad.GmailProfesor))
            {
                var usuarioProfesor = _usuarioRepo.ObtenerUsuarioPorCorreo(actividad.GmailProfesor);
                if (usuarioProfesor != null)
                {
                    actividad.IdResponsable = usuarioProfesor.IdUsuario;
                }
            }

            ViewBag.ListaProfesores = _usuarioRepo.ObtenerProfesores();
            return View(actividad);
        }

        [Authorize(Roles = "Administrador,Profesor")]
        [HttpPost]
        public IActionResult Editar(Actividad actividad)
        {
            string idAuditoria = User.FindFirstValue("IdUsuario");

            if (!string.IsNullOrEmpty(actividad.Codigo) && !string.IsNullOrEmpty(actividad.Nombre) && !string.IsNullOrEmpty(actividad.IdResponsable))
            {
                _service.Actualizar(actividad, idAuditoria);
                return RedirectToAction(nameof(Index));
            }

            ViewBag.ListaProfesores = _usuarioRepo.ObtenerProfesores();
            return View(actividad);
        }

        [Authorize(Roles = "Administrador,Profesor")]
        [HttpPost]
        public IActionResult Eliminar(string id)
        {
            string idAuditoria = User.FindFirstValue("IdUsuario");
            if (!string.IsNullOrEmpty(id))
            {
                _service.Eliminar(id, idAuditoria);
            }
            return RedirectToAction(nameof(Index));
        }

        // ... (Estadísticas se mantiene igual, usando el método nuevo del Repo) ...
        [Authorize(Roles = "Administrador,Profesor")]
        public IActionResult Estadisticas()
        {
            var actividades = _service.ObtenerTodas();
            var agrupado = actividades
                .GroupBy(a => a.TipoDiscapacidad)
                .Select(g => new { Label = string.IsNullOrEmpty(g.Key) ? "General" : g.Key, Cantidad = g.Count(), CupoTotal = g.Sum(x => x.Cupo) }).ToList();

            ViewBag.Labels = agrupado.Select(x => x.Label).ToArray();
            ViewBag.DataCantidad = agrupado.Select(x => x.Cantidad).ToArray();
            ViewBag.DataCupos = agrupado.Select(x => x.CupoTotal).ToArray();

            ViewBag.TotalActividades = actividades.Count;
            ViewBag.TotalCupos = actividades.Sum(a => a.Cupo);
            ViewBag.UsuarioTotal = _usuarioRepo.ContarUsuarios();

            return View();
        }
    }
}