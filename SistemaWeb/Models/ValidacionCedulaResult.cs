namespace SistemaWeb.Models
{
    public class ValidacionCedulaResult
    {
        public bool EsValida { get; set; }
        // Agregamos el signo de pregunta '?' para que acepte nulos y no de error
        public string? Mensaje { get; set; }
        public bool EsEstudiante { get; set; }
        public string? Cedula { get; set; }
        public string? Nombre { get; set; }

        public object? DatosEstudiante { get; set; }
    }
}