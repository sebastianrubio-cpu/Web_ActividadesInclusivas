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
                // CORRECCIÓN: La ruta debe coincidir con la definida en tu API (Program.cs)
                // Antes tenías: api/usuarios/verificar/
                // Ahora es: api/estudiantes/
                var response = await _httpClient.GetAsync($"{_baseUrl}/api/estudiantes/{cedula}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<ValidacionCedulaResult>(content);

                    if (resultado != null)
                    {
                        // Sincronizamos EsValida con lo que devolvió la API
                        resultado.EsValida = resultado.EsEstudiante;
                        return resultado;
                    }

                    return new ValidacionCedulaResult { EsValida = false, Mensaje = "Respuesta vacía del servidor." };
                }

                // Si la API devuelve 404 u otro error
                return new ValidacionCedulaResult { EsValida = false, Mensaje = "Cédula no encontrada en el sistema." };
            }
            catch (Exception ex)
            {
                return new ValidacionCedulaResult { EsValida = false, Mensaje = $"Error de conexión: {ex.Message}" };
            }
        }
    }
}