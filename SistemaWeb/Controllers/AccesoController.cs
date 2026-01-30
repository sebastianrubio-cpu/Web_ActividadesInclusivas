using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

namespace SistemaWeb.Controllers
{
    public class AccesoController : Controller
    {
        private readonly UsuarioRepository _usuarioRepository;

        public AccesoController(UsuarioRepository usuarioRepository)
        {
            _usuarioRepository = usuarioRepository;
        }

        public IActionResult Login()
        {
            // Si ya está logueado, redirigir al inicio
            if (User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(string correo, string clave)
        {
            Usuario usuario = _usuarioRepository.ValidarUsuario(correo, clave);

            if (usuario != null)
            {
                // 1. CREAR LOS CLAIMS (Datos del usuario en la cookie)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, usuario.Correo),
                    new Claim("NombreCompleto", usuario.Nombre),
                    
                    // ¡IMPORTANTE! Esta línea permite que User.IsInRole("Estudiante") funcione
                    new Claim(ClaimTypes.Role, usuario.Rol)
                };

                // 2. CREAR LA IDENTIDAD
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                // 3. INICIAR SESIÓN (Crear la cookie)
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction("Index", "Home");
            }
            else
            {
                ViewBag.Error = "Correo o clave incorrectos";
                return View();
            }
        }

        public async Task<IActionResult> Salir()
        {
            // BORRAR LA COOKIE
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Acceso");
        }



        // GET: Mostrar formulario de registro
        public IActionResult Registrar()
        {
            if (User.Identity!.IsAuthenticated) return RedirectToAction("Index", "Home");

            // Cargar géneros para el select
            ViewBag.Generos = _usuarioRepository.ObtenerGeneros();
            return View();
        }

        // POST: Procesar registro
        [HttpPost]
        public IActionResult Registrar(string cedula, string nombre, string correo, string clave, int idGenero)
        {
            // 1. Validaciones básicas
            if (!correo.EndsWith("@uisek.edu.ec"))
            {
                ViewBag.Error = "El correo debe ser institucional (@uisek.edu.ec)";
                ViewBag.Generos = _usuarioRepository.ObtenerGeneros();
                return View();
            }

            // 2. Intentar registrar
            bool resultado = _usuarioRepository.RegistrarEstudiante(cedula, nombre, correo, clave, idGenero);

            if (resultado)
            {
                // Éxito: Redirigir al Login
                return RedirectToAction("Login");
            }
            else
            {
                // Error (Duplicado)
                ViewBag.Error = "No se pudo registrar. Verifique que la Cédula o Correo no existan ya.";
                ViewBag.Generos = _usuarioRepository.ObtenerGeneros();
                return View();
            }
        }


    }
}