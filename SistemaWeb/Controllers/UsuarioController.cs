using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SistemaWeb.Models;

namespace SistemaWeb.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UsuarioRepository _usuarioRepo;

        public UsuariosController(UsuarioRepository usuarioRepo)
        {
            _usuarioRepo = usuarioRepo;
        }

        // LISTAR
        public IActionResult Index()
        {
            var usuarios = _usuarioRepo.ObtenerTodos();
            return View(usuarios);
        }

        // --- CREAR ---
        [HttpGet]
        public IActionResult Crear()
        {
            CargarListas();
            return View();
        }

        [HttpPost]
        public IActionResult Crear(Usuario usuario)
        {
            // Validamos lo básico manualmente para evitar problemas con ModelState
            if (!string.IsNullOrWhiteSpace(usuario.IdUsuario) &&
                !string.IsNullOrWhiteSpace(usuario.Nombre) &&
                !string.IsNullOrWhiteSpace(usuario.Clave))
            {
                bool resultado = _usuarioRepo.CrearUsuarioConRol(usuario);
                if (resultado)
                    return RedirectToAction("Index");
                else
                    ModelState.AddModelError("", "Error: La cédula o el correo ya existen.");
            }

            CargarListas();
            return View(usuario);
        }

        // --- EDITAR ---
        [HttpGet]
        public IActionResult Editar(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var usuario = _usuarioRepo.ObtenerPorId(id);
            if (usuario == null) return NotFound();

            CargarListas();
            return View(usuario);
        }

        [HttpPost]
        public IActionResult Editar(Usuario usuario)
        {
            if (!string.IsNullOrWhiteSpace(usuario.Nombre) &&
                !string.IsNullOrWhiteSpace(usuario.Clave))
            {
                bool resultado = _usuarioRepo.Actualizar(usuario);
                if (resultado)
                    return RedirectToAction("Index");
                else
                    ModelState.AddModelError("", "No se pudo actualizar el usuario.");
            }

            CargarListas();
            return View(usuario);
        }

        // --- ELIMINAR ---
        [HttpPost]
        public IActionResult Eliminar(string id)
        {
            if (!string.IsNullOrEmpty(id))
            {
                _usuarioRepo.Eliminar(id);
            }
            return RedirectToAction("Index");
        }

        // Auxiliar para llenar los Dropdowns
        private void CargarListas()
        {
            var roles = _usuarioRepo.ObtenerListaRoles();
            var generos = _usuarioRepo.ObtenerGeneros();

            ViewBag.Roles = new SelectList(roles, "IdRol", "NombreRol");
            ViewBag.Generos = new SelectList(generos, "Key", "Value");
        }
    }
}