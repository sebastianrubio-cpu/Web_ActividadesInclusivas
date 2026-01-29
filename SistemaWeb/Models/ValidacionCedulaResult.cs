namespace SistemaWeb.Models
{
    public class ValidacionCedulaResult
    {
        // 1. Esta propiedad recibe el dato que viene de la API (JSON: "esEstudiante")
        public bool EsEstudiante { get; set; }
        public bool EsValida { get; set; }
        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Mensaje { get; set; }
    }
}