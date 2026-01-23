using Microsoft.Data.SqlClient; // <--- CORREGIDO (Antes era System.Data)
using System.Data;

namespace SistemaWeb.Models
{
    public class Usuario
    {
        public int IdUsuario { get; set; }
        public string Correo { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
    }

    public class UsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public Usuario ValidarUsuario(string correo, string clave)
        {
            Usuario usuario = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT * FROM Usuarios WHERE Correo = @Correo AND Clave = @Clave", connection))
                {
                    command.Parameters.AddWithValue("@Correo", correo);
                    command.Parameters.AddWithValue("@Clave", clave);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                IdUsuario = (int)reader["IdUsuario"],
                                Correo = reader["Correo"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Rol = reader["Rol"].ToString()
                            };
                        }
                    }
                }
            }
            return usuario;
        }
    }
}
