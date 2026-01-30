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
            _baseUrl = configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7199";
        }

        public async Task<ValidacionCedulaResult> ConsultarPorCedulaAsync(string cedula)
        {
            try
            {
                // Verifica que la URL base no tenga barra al final para evitar dobles barras
                var url = $"{_baseUrl.TrimEnd('/')}/api/estudiantes/{cedula}";
                var response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<ValidacionCedulaResult>(content);

                    if (resultado != null)
                    {
                        resultado.EsValida = resultado.EsEstudiante;
                        return resultado;
                    }
                    return new ValidacionCedulaResult { EsValida = false, Mensaje = "Respuesta vacía del servidor." };
                }

                // --- MEJORA DE DIAGNÓSTICO ---
                // Si falla, devolvemos el código de error exacto (ej: 500 Internal Server Error)
                return new ValidacionCedulaResult
                {
                    EsValida = false,
                    Mensaje = $"Error del Servidor API: {response.StatusCode} ({response.ReasonPhrase})"
                };
            }
            catch (Exception ex)
            {
                return new ValidacionCedulaResult { EsValida = false, Mensaje = $"Error de Conexión: {ex.Message}" };
            }
        }
    }
}