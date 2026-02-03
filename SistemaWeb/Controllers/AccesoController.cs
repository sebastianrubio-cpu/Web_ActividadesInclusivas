using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SistemaWeb.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq; // Necesario para validar contraseña

namespace SistemaWeb.Controllers
{
    public class AccesoController : Controller
    {
        private readonly UsuarioRepository _usuarioRepo;

        public AccesoController(UsuarioRepository usuarioRepo)
        {
            _usuarioRepo = usuarioRepo;
        }

        // GET: Mostrar Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(User.IsInRole("Estudiante") ? "VistaEstudiante" : "Index", "Actividades");
            }

            // Usamos ViewData para evitar errores dinámicos en Razor
            ViewData["Generos"] = _usuarioRepo.ObtenerGeneros();
            return View();
        }

        // POST: Procesar Login
        [HttpPost]
        public async Task<IActionResult> Login(string Correo, string Clave)
        {
            Usuario usuario = _usuarioRepo.ValidarUsuario(Correo, Clave);

            if (usuario != null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim("Correo", usuario.Correo),
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

        // POST: Procesar Registro
        [HttpPost]
        public IActionResult Registrar(Usuario usuario)
        {
            // 1. Asignar Rol por defecto
            usuario.Rol = "Estudiante";

            // 2. Quitar 'Rol' de la validación automática
            ModelState.Remove("Rol");

            // 3. Validar Seguridad de Contraseña
            if (!EsClaveSegura(usuario.Clave))
            {
                ViewData["ErrorRegistro"] = "La contraseña debe tener 8 caracteres, una mayúscula y un signo especial.";
                ViewData["Generos"] = _usuarioRepo.ObtenerGeneros();
                ViewData["MostrarRegistro"] = true; // Mantiene el panel abierto
                return View("Login", usuario);
            }

            // 4. Intentar guardar
            if (ModelState.IsValid)
            {
                bool resultado = _usuarioRepo.Registrar(usuario);
                if (resultado)
                {
                    ViewData["Exito"] = "Cuenta creada correctamente. Inicie sesión.";
                    ViewData["Generos"] = _usuarioRepo.ObtenerGeneros();
                    // Al ser exitoso, NO enviamos 'MostrarRegistro' para que muestre el Login limpio
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

            // Si falló algo, recargamos la vista manteniendo el panel de registro abierto
            ViewData["Generos"] = _usuarioRepo.ObtenerGeneros();
            ViewData["MostrarRegistro"] = true;
            return View("Login", usuario);
        }

        // SALIR
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login");
        }

        // --- MÉTODO PRIVADO DE VALIDACIÓN ---
        private bool EsClaveSegura(string clave)
        {
            if (string.IsNullOrEmpty(clave)) return false;
            if (clave.Length < 8) return false;
            if (!clave.Any(char.IsUpper)) return false; // Al menos una mayúscula
            if (!clave.Any(ch => !char.IsLetterOrDigit(ch))) return false; // Al menos un símbolo
            return true;
        }
    }
}