using System.ComponentModel.DataAnnotations;

namespace SistemaWeb.Models
{
    public class Usuario
    {
        [Required(ErrorMessage = "La cédula es obligatoria")]
        public string IdUsuario { get; set; } // Cédula (PK)

        [Required, EmailAddress]
        public string Correo { get; set; }

        [Required]
        public string Clave { get; set; }

        [Required]
        public string Nombre { get; set; }

        public int IdRol { get; set; }
        public string? Rol { get; set; }

        public int? IdGenero { get; set; }
    }
}