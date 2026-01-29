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

        // 1. VALIDAR LOGIN (Ya lo debías tener, pero por si acaso)
        public Usuario ValidarUsuario(string correo, string clave)
        {
            Usuario usuario = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT Nombre, Rol, Correo FROM Usuarios WHERE Correo = @Correo AND Clave = @Clave";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Correo", correo);
                    cmd.Parameters.AddWithValue("@Clave", clave);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                Nombre = reader["Nombre"].ToString(),
                                Rol = reader["Rol"].ToString(),
                                Correo = reader["Correo"].ToString()
                            };
                        }
                    }
                }
            }
            return usuario;
        }

        // 2. REGISTRAR ESTUDIANTE (NUEVO)
        public bool RegistrarEstudiante(string nombre, string correo, string clave)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // Rol 'Estudiante' hardcodeado como pediste
                    string sql = "INSERT INTO Usuarios (Nombre, Correo, Clave, Rol) VALUES (@Nombre, @Correo, @Clave, 'Estudiante')";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", nombre);
                        cmd.Parameters.AddWithValue("@Correo", correo);
                        cmd.Parameters.AddWithValue("@Clave", clave);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch
            {
                return false; // Probablemente el correo ya existe (Unique Constraint)
            }
        }

        // 3. RECUPERAR CLAVE (NUEVO)
        public string ObtenerClavePorCorreo(string correo)
        {
            string clave = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT Clave FROM Usuarios WHERE Correo = @Correo";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Correo", correo);
                    var result = cmd.ExecuteScalar();
                    if (result != null)
                    {
                        clave = result.ToString();
                    }
                }
            }
            return clave;
        }
    }
}
