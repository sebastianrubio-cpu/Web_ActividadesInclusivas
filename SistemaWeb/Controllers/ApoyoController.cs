using Microsoft.AspNetCore.Mvc;
using SistemaWeb.Services;
using SistemaWeb.Models;

namespace SistemaWeb.Controllers
{
    public class ApoyoController : Controller
    {
        private readonly EstudiantesClient _apiClient;

        public ApoyoController(EstudiantesClient apiClient)
        {
            _apiClient = apiClient;
        }

        // GET: Muestra la vista de búsqueda
        [HttpGet]
        public IActionResult ValidarCedula()
        {
            return View();
        }

        // POST: Procesa la búsqueda
        [HttpPost]
        public async Task<IActionResult> ResultadoCedula(string cedula)
        {
            if (string.IsNullOrEmpty(cedula))
            {
                ViewBag.Error = "Debe ingresar un número de cédula.";
                return View("ValidarCedula");
            }

            try
            {
                // Llama a la API (que ahora busca en IdUsuario)
                ValidacionCedulaResult resultado = await _apiClient.ConsultarPorCedulaAsync(cedula);
                return View(resultado);
            }
            catch (Exception ex)
            {
                ViewBag.Error = "Error de conexión: " + ex.Message;
                return View("ValidarCedula");
            }
        }
    }
}