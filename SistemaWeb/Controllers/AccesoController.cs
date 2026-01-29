using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Models;
using SistemaWeb.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace SistemaWeb.Controllers
{
    public class AccesoController : Controller
    {
        private readonly UsuarioRepository _usuarioRepository;
        private readonly CorreoService _correoService; // <--- Inyectamos CorreoService

        // Constructor actualizado
        public AccesoController(UsuarioRepository usuarioRepository, CorreoService correoService)
        {
            _usuarioRepository = usuarioRepository;
            _correoService = correoService;
        }

        // 1. LOGIN (VISTA)
        public IActionResult Login()
        {
            // TU MEJORA: Si ya está logueado, mandarlo directo al sistema
            ClaimsPrincipal claimUser = HttpContext.User;
            if (claimUser.Identity.IsAuthenticated)
            {
                return RedirectToAction("Index", "Actividades");
            }
            return View();
        }

        // 2. LOGIN (POST)
        [HttpPost]
        public async Task<IActionResult> Login(string correo, string clave)
        {
            Usuario usuario = _usuarioRepository.ValidarUsuario(correo, clave);

            if (usuario != null)
            {
                var claims = new List<Claim> {
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim(ClaimTypes.Email, usuario.Correo),
                    new Claim(ClaimTypes.Role, usuario.Rol)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Actividades");
            }
            else
            {
                ViewBag.Error = "Usuario o contraseña incorrectos"; // Usamos ViewBag para mostrarlo en la alerta roja
                return View();
            }
        }

        // 3. SALIR
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Acceso");
        }

        // 4. REGISTRAR (NUEVO)
        [HttpPost]
        public IActionResult Registrar(string nombre, string correo, string clave)
        {
            if (string.IsNullOrEmpty(nombre) || string.IsNullOrEmpty(correo) || string.IsNullOrEmpty(clave))
            {
                ViewBag.ErrorRegistro = "Todos los campos son obligatorios";
                return View("Login");
            }

            bool resultado = _usuarioRepository.RegistrarEstudiante(nombre, correo, clave);

            if (resultado)
            {
                ViewBag.Mensaje = "¡Cuenta creada! Inicie sesión.";
                return View("Login");
            }
            else
            {
                ViewBag.ErrorRegistro = "El correo ya está registrado o hubo un error.";
                return View("Login");
            }
        }

        // 5. RECUPERAR CLAVE (NUEVO)
        [HttpPost]
        public IActionResult RecuperarClave(string correoRecuperacion)
        {
            string clave = _usuarioRepository.ObtenerClavePorCorreo(correoRecuperacion);

            if (string.IsNullOrEmpty(clave))
            {
                TempData["ErrorRecuperacion"] = "El correo no existe en el sistema.";
            }
            else
            {
                string mensaje = $@"
                    <h1>Recuperación de Contraseña</h1>
                    <p>Su contraseña actual es: <strong>{clave}</strong></p>
                    <p>Atte. UISEK Inclusiva</p>";

                bool enviado = _correoService.EnviarCorreo(correoRecuperacion, "Recuperación Clave", mensaje);

                if (enviado)
                    TempData["ExitoRecuperacion"] = "Contraseña enviada a su correo.";
                else
                    TempData["ErrorRecuperacion"] = "Error al enviar el correo.";
            }

            return RedirectToAction("Login");
        }
    }
}
