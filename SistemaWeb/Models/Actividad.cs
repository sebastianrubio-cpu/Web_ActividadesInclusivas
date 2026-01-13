using System.ComponentModel.DataAnnotations;

namespace SistemaWeb.Models
{
    public class Actividad
    {
        [Key]
        [Required(ErrorMessage = "El código es obligatorio")]
        public string? Codigo { get; set; } // Esta es la clave que faltaba

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime FechaRealizacion { get; set; }

        public string? TipoDiscapacidad { get; set; }

        [Required(ErrorMessage = "El cupo es obligatorio")]
        public int Cupo { get; set; }

        public string? Responsable { get; set; }

        public string? Estado { get; set; }
    }
}