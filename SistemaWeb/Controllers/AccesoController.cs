using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SistemaWeb.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace SistemaWeb.Controllers
{
    public class AccesoController : Controller
    {
        private readonly UsuarioRepository _usuarioRepo;

        public AccesoController(UsuarioRepository usuarioRepo)
        {
            _usuarioRepo = usuarioRepo;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(User.IsInRole("Estudiante") ? "VistaEstudiante" : "Index", "Actividades");
            }
            ViewData["Generos"] = _usuarioRepo.ObtenerGeneros();
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string Correo, string Clave)
        {
            Usuario usuario = _usuarioRepo.ValidarUsuario(Correo, Clave);

            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    // CRÍTICO: Guardar ID Usuario para la Auditoría
                    new Claim("IdUsuario", usuario.IdUsuario),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim("Correo", usuario.Correo),
                    // Usamos usuario.Rol que viene lleno con el nombre ("Administrador", etc.)
                    new Claim(ClaimTypes.Role, usuario.Rol)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction(usuario.Rol == "Estudiante" ? "VistaEstudiante" : "Index", "Actividades");
            }

            ViewData["Error"] = "Correo o contraseña incorrectos";
            ViewData["Generos"] = _usuarioRepo.ObtenerGeneros();
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(Usuario usuario)
        {
            // El rol se asigna en BD (IdRol = 3), limpiamos validación
            ModelState.Remove("Rol");
            ModelState.Remove("IdRol");

            if (!EsClaveSegura(usuario.Clave))
            {
                ViewData["ErrorRegistro"] = "La contraseña debe tener 8 caracteres, una mayúscula y un signo especial.";
                ViewData["Generos"] = _usuarioRepo.ObtenerGeneros();
                ViewData["MostrarRegistro"] = true;
                return View("Login", usuario);
            }

            if (ModelState.IsValid)
            {
                bool resultado = _usuarioRepo.Registrar(usuario);
                if (resultado)
                {
                    ViewData["Exito"] = "Cuenta creada correctamente. Inicie sesión.";
                    ViewData["Generos"] = _usuarioRepo.ObtenerGeneros();
                    return View("Login");
                }
                else
                {
                    ViewData["ErrorRegistro"] = "La Cédula o el Correo ya están registrados.";
                }
            }
            else
            {
                ViewData["ErrorRegistro"] = "Por favor complete todos los campos obligatorios.";
            }

            ViewData["Generos"] = _usuarioRepo.ObtenerGeneros();
            ViewData["MostrarRegistro"] = true;
            return View("Login", usuario);
        }

        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        private bool EsClaveSegura(string clave)
        {
            if (string.IsNullOrEmpty(clave)) return false;
            if (clave.Length < 8) return false;
            if (!clave.Any(char.IsUpper)) return false;
            if (!clave.Any(ch => !char.IsLetterOrDigit(ch))) return false;
            return true;
        }
    }
}