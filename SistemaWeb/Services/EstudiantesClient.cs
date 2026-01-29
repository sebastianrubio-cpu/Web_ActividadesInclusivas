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

        public async Task<ValidacionCedulaResult> ConsultarPorCedulaAsync(string cedula)
        {
            try
            {
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/estudiantes/{cedula}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<ValidacionCedulaResult>(content);

                    if (resultado != null)
                    {
                        resultado.EsValida = resultado.EsEstudiante;
                        return resultado;
                    }

                    return new ValidacionCedulaResult { EsValida = false, Mensaje = "Respuesta vacía" };
                }

                return new ValidacionCedulaResult { EsValida = false, Mensaje = "Cédula no encontrada o error en API" };
            }
            catch (Exception ex)
            {
                return new ValidacionCedulaResult { EsValida = false, Mensaje = $"Error de conexión: {ex.Message}" };
            }
        }
    }
}