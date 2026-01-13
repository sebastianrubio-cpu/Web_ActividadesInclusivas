using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Services;
using SistemaWeb.Models;

namespace SistemaWeb.Controllers
{
    public class ApoyoController : Controller
    {
        private readonly EstudiantesClient _cliente;

        public ApoyoController(EstudiantesClient cliente)
        {
            _cliente = cliente;
        }

        // GET: Vista del formulario
        public IActionResult ValidarCedula()
        {
            return View();
        }

        // POST: Enviamos la cédula a la API
        [HttpPost]
        public async Task<IActionResult> ValidarCedula(string cedula)
        {
            if (string.IsNullOrEmpty(cedula))
            {
                ViewBag.Error = "⚠️ Por favor ingrese un número de identificación.";
                return View();
            }

            // Llamamos a la API
            var resultado = await _cliente.ConsultarPorCedulaAsync(cedula);

            return View(resultado);
        }
    }
}