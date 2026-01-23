using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using SistemaWeb.Services;

namespace SistemaWeb.Models
{
    public class ValidarSistema
    {
        public string EstadoApi { get; set; }
        public bool ApiOnline { get; set; }
        public string SistemaOperativo { get; set; }
        public string Fecha { get; set; }
        public string Hora { get; set; }
        public string Lugar { get; set; }

        public static async Task<ValidarSistema> RealizarChequeo(EstudiantesClient clienteApi)
        {
            var reporte = new ValidarSistema
            {
                SistemaOperativo = RuntimeInformation.OSDescription,
                Fecha = DateTime.Now.ToString("dd/MM/yyyy"),
                Hora = DateTime.Now.ToString("HH:mm:ss"),
                Lugar = Environment.MachineName
            };

            try
            {

                await clienteApi.ConsultarPorCedulaAsync("TEST_PING");
                reporte.ApiOnline = true;
                reporte.EstadoApi = "🟡 API En Línea (Local)";
            }
            catch
            {
                reporte.ApiOnline = false;
                reporte.EstadoApi = "🔴 Desconectado";
            }

            return reporte;
        }
    }
}