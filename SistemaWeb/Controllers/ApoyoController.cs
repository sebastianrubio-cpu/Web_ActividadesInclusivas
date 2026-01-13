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

        public IActionResult ValidarCedula()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResultadoCedula(string cedula)
        {
            if (string.IsNullOrEmpty(cedula))
            {
                ViewBag.Error = "Debe ingresar una cédula";
                return View("ValidarCedula");
            }

            var resultado = await _cliente.ConsultarPorCedulaAsync(cedula);
            return View(resultado);
        }
    }
}