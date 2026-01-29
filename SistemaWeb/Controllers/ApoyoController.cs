using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Services;
using SistemaWeb.Models;
using Microsoft.AspNetCore.Authorization; // <--- Importante

namespace SistemaWeb.Controllers
{
    // BLOQUEAMOS TODO EL CONTROLADOR A ESTUDIANTES
    // Solo Admins y Profesores pueden usar estas herramientas
    [Authorize(Roles = "Administrador,Profesor")]
    public class ApoyoController : Controller
    {
        private readonly EstudiantesClient _cliente;

        public ApoyoController(EstudiantesClient cliente)
        {
            _cliente = cliente;
        }

        [HttpGet]
        public IActionResult ValidarCedula()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ValidarCedula(string cedula)
        {
            if (string.IsNullOrEmpty(cedula))
            {
                ViewBag.Error = "Debe ingresar una cédula.";
                return View();
            }

            var resultado = await _cliente.ConsultarPorCedulaAsync(cedula);

            // Pasamos el resultado a la vista de respuesta
            return View("ResultadoCedula", resultado);
        }
    }
}