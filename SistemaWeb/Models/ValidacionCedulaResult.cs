namespace SistemaWeb.Models
{
    public class ValidacionCedulaResult
    {
        public bool EsEstudiante { get; set; }

        // CORRECCIÓN: Se cambia a { get; set; } para eliminar el error "read only"
        public bool EsValida { get; set; }

        public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Mensaje { get; set; }
    }
}