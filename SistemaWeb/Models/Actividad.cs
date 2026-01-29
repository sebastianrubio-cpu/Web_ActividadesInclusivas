using System.ComponentModel.DataAnnotations;

namespace SistemaWeb.Models
{
    public class Actividad
    {
        [Key]
        [Required(ErrorMessage = "El código es obligatorio")]

        [RegularExpression(@"^[A-Z][0-9]+$", ErrorMessage = "El código debe iniciar con una Mayúscula seguido de números (Ej: A001).")]
        public string? Codigo { get; set; }

        [Required(ErrorMessage = "El nombre es obligatorio")]
        public string? Nombre { get; set; }

        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime FechaRealizacion { get; set; }

        public string? TipoDiscapacidad { get; set; }

        [Required(ErrorMessage = "El cupo es obligatorio")]
        public int Cupo { get; set; }

        public string? Responsable { get; set; }

        public string? Estado { get; set; }

        public string? GmailProfesor { get; set; }

        public double Latitud { get; set; }  
        public double Longitud { get; set; }


    }
}