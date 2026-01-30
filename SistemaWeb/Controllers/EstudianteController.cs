using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SistemaWeb.Models;
using System.Security.Claims;

namespace SistemaWeb.Controllers
{
    [Authorize(Roles = "Estudiante")]
    public class EstudianteController : Controller
    {
        private readonly UsuarioRepository _repo;

        public EstudianteController(UsuarioRepository repo)
        {
            _repo = repo;
        }

        [HttpGet]
        public IActionResult ActualizarPerfil()
        {
            // 1. Obtenemos el correo de la sesión actual
            var correo = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity.Name;
            
            // 2. CORRECCIÓN: Usamos el método que NO pide contraseña
            var usuario = _repo.ObtenerUsuarioPorCorreo(correo);

            // 3. Si por alguna razón no existe, redirigimos al login en vez de lanzar error
            if (usuario == null)
            {
                return RedirectToAction("Login", "Acceso");
            }

            return View(usuario);
        }

        [HttpPost]
        public IActionResult GuardarPerfil(Usuario model)
        {
            if (_repo.ActualizarPerfilEstudiante(model))
            {
                TempData["Mensaje"] = "Datos actualizados correctamente.";
                return RedirectToAction("Index", "Home");
            }

            ViewBag.Error = "Error: El correo debe terminar en @uisek.edu.ec";
            return View("ActualizarPerfil", model);
        }
    }
}