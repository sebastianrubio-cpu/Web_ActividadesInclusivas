using Newtonsoft.Json;
using SistemaWeb.Models;

namespace SistemaWeb.Services
{
    public class EstudiantesClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public EstudiantesClient(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7000";
        }

        // Renombrado para que coincida con la llamada del controlador y la nueva lógica
        public async Task<ValidacionCedulaResult> ConsultarPorCedulaAsync(string cedula)
        {
            try
            {
                // Ahora apuntamos al endpoint de usuarios en lugar del de estudiantes
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/usuarios/verificar/{cedula}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<ValidacionCedulaResult>(content);

                    if (resultado != null)
                    {
                        // Mantenemos la propiedad EsValida para compatibilidad con la vista
                        resultado.EsValida = resultado.EsEstudiante;
                        return resultado;
                    }

                    return new ValidacionCedulaResult { EsValida = false, Mensaje = "Respuesta vacía" };
                }

                return new ValidacionCedulaResult { EsValida = false, Mensaje = "Cédula no encontrada en el sistema de usuarios" };
            }
            catch (Exception ex)
            {
                return new ValidacionCedulaResult { EsValida = false, Mensaje = $"Error de conexión: {ex.Message}" };
            }
        }
    }
}