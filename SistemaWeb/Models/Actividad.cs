using System.ComponentModel.DataAnnotations;

namespace SistemaWeb.Models
{
    public class Actividad
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "El título es obligatorio")]
        public string? Titulo { get; set; } // <--- ¡Mira el signo ?!

        [Required(ErrorMessage = "La descripción es obligatoria")]
        public string? Descripcion { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime Fecha { get; set; }

        [Required(ErrorMessage = "El lugar es obligatorio")]
        public string? Lugar { get; set; }
    }
}