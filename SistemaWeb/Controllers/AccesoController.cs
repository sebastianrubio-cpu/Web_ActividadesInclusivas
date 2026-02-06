using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SistemaWeb.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using SistemaWeb.Services; // Asegúrate de tener este using

namespace SistemaWeb.Controllers
{
    public class AccesoController : Controller
    {
        private readonly UsuarioRepository _usuarioRepo;
        private readonly CorreoService _correoService;

        public AccesoController(UsuarioRepository usuarioRepo, CorreoService correoService)
        {
            _usuarioRepo = usuarioRepo;
            _correoService = correoService;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction(User.IsInRole("Estudiante") ? "VistaEstudiante" : "Index", "Actividades");
            }
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
                    new Claim("IdUsuario", usuario.IdUsuario),
                    new Claim(ClaimTypes.Name, usuario.Nombre),
                    new Claim("Correo", usuario.Correo),
                    new Claim(ClaimTypes.Role, usuario.Rol)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                return RedirectToAction(usuario.Rol == "Estudiante" ? "VistaEstudiante" : "Index", "Actividades");
            }

            ViewData["Error"] = "Correo o contraseña incorrectos";
            // No activamos MostrarRegistro aquí porque el error fue en el Login
            return View();
        }

        [HttpPost]
        public IActionResult Registrar(Usuario usuario)
        {
            // Limpiamos validaciones de rol porque se asigna por defecto
            ModelState.Remove("Rol");
            ModelState.Remove("IdRol");

            if (!EsClaveSegura(usuario.Clave))
            {
                ViewData["ErrorRegistro"] = "La contraseña debe tener 8 caracteres, una mayúscula y un signo especial.";
                ViewData["MostrarRegistro"] = true; // <--- ESTO MANTIENE EL PANEL ABIERTO
                return View("Login");
            }

            if (ModelState.IsValid)
            {
                bool resultado = _usuarioRepo.Registrar(usuario);
                if (resultado)
                {
                    ViewData["Exito"] = "Cuenta creada correctamente. Inicie sesión.";
                    // Aquí NO ponemos MostrarRegistro=true para que el usuario vea el panel de Login
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

            ViewData["MostrarRegistro"] = true; // Si falló algo, mantenemos el panel de registro visible
            return View("Login", usuario);
        }

        [HttpPost]
        public IActionResult RecuperarClave(string correoRecuperacion)
        {
            var usuario = _usuarioRepo.ObtenerUsuarioPorCorreo(correoRecuperacion);

            // Validamos usuario y Rol (2=Profesor, 3=Estudiante)
            if (usuario != null && (usuario.IdRol == 2 || usuario.IdRol == 3))
            {
                string asunto = "UISEK - Recuperación de Credenciales";

                // Mensaje HTML Profesional
                string mensaje = $@"
                    <html>
                    <head>
                        <style>
                            .container {{ font-family: 'Arial', sans-serif; max-width: 600px; margin: 0 auto; color: #333; }}
                            .header {{ background-color: #002d5b; padding: 20px; text-align: center; color: white; }}
                            .content {{ padding: 20px; background-color: #f9f9f9; border: 1px solid #ddd; }}
                            .pass-box {{ background-color: #e3f2fd; padding: 15px; border-left: 5px solid #004a99; margin: 20px 0; font-family: monospace; font-size: 18px; }}
                            .footer {{ font-size: 12px; color: #777; margin-top: 20px; text-align: center; border-top: 1px solid #eee; padding-top: 10px; }}
                            .warning {{ color: #d32f2f; font-weight: bold; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Recuperación de Contraseña</h2>
                            </div>
                            <div class='content'>
                                <p>Estimado usuario,</p>
                                <p>Usted ha solicitado su contraseña debido a la pérdida de la misma.</p>
                                
                                <p>A continuación se detallan sus credenciales actuales:</p>
                                <div class='pass-box'>
                                    {usuario.Clave}
                                </div>
                                
                                <p class='warning'>Importante:</p>
                                <p>Le recomendamos solicitar el cambio de esta contraseña en el departamento de Administración para garantizar la seguridad de su cuenta.</p>
                            </div>
                            <div class='footer'>
                                <p>Si usted no realizó esta solicitud, por favor contáctese inmediatamente con administración vía 
                                <a href='mailto:admin@uisek.edu.ec'>admin@uisek.edu.ec</a> o acérquese a nuestras oficinas.</p>
                                <p>Universidad Internacional SEK</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                _correoService.EnviarCorreo(usuario.Correo, asunto, mensaje);

                // Usamos la clave EXACTA que pusiste en la vista
                TempData["ExitoRecuperacion"] = "Se han enviado las instrucciones a su correo.";
            }
            else
            {
                // Mensaje de error si no existe o no tiene permisos
                TempData["ErrorRecuperacion"] = "No pudimos procesar su solicitud. Verifique el correo o contacte a soporte.";
            }

            return RedirectToAction("Login");
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