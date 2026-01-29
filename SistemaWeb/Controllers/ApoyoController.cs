using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Services;
using SistemaWeb.Models;
using System.Threading.Tasks;

namespace SistemaWeb.Controllers
{
    public class ApoyoController : Controller
    {
        private readonly EstudiantesClient _cliente;

        public ApoyoController(EstudiantesClient cliente)
        {
            _cliente = cliente;
        }

        // GET: Muestra el formulario vacío
        public IActionResult ValidarCedula()
        {
            return View();
        }

        // POST: Procesa la búsqueda
        [HttpPost]
        public async Task<IActionResult> ValidarCedula(string cedula)
        {
            if (string.IsNullOrEmpty(cedula))
            {
                ViewBag.Error = "⚠️ Por favor ingrese un número de identificación.";
                return View();
            }

            // 1. Consultamos la API
            var resultado = await _cliente.ConsultarPorCedulaAsync(cedula);

            // 2. [CORRECCIÓN] Redirigimos a la vista de RESULTADOS enviando el modelo
            return View("ResultadoCedula", resultado);
        }
    }
}