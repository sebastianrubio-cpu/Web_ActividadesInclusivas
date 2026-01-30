using Microsoft.Data.SqlClient;
using System.Data;

namespace SistemaWeb.Models
{
    public class Usuario
    {
        public string IdUsuario { get; set; } // Cédula (PK)
        public string Correo { get; set; }
        public string Clave { get; set; }
        public string Nombre { get; set; }
        public string Rol { get; set; }
        public int? IdGenero { get; set; }
    }

    public class UsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // 1. VALIDAR LOGIN
        public Usuario ValidarUsuario(string correo, string clave)
        {
            Usuario usuario = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT IdUsuario, Nombre, Rol, Correo, IdGenero FROM Usuarios WHERE Correo = @Correo AND Clave = @Clave";
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
                                IdUsuario = reader["IdUsuario"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Rol = reader["Rol"].ToString(),
                                Correo = reader["Correo"].ToString(),
                                IdGenero = reader["IdGenero"] != DBNull.Value ? Convert.ToInt32(reader["IdGenero"]) : null
                            };
                        }
                    }
                }
            }
            return usuario;
        }

        // 2. REGISTRAR ESTUDIANTE (Cédula es el IdUsuario)
        public bool RegistrarEstudiante(string cedula, string nombre, string correo, string clave, int idGenero)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO Usuarios (IdUsuario, Nombre, Correo, Clave, Rol, IdGenero) 
                                   VALUES (@IdUsuario, @Nombre, @Correo, @Clave, 'Estudiante', @IdGenero)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", cedula);
                        cmd.Parameters.AddWithValue("@Nombre", nombre);
                        cmd.Parameters.AddWithValue("@Correo", correo);
                        cmd.Parameters.AddWithValue("@Clave", clave);
                        cmd.Parameters.AddWithValue("@IdGenero", idGenero);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch
            {
                return false; // Error por duplicado
            }
        }

        // 3. OBTENER POR CORREO (Necesario para cargar la vista de edición)
        public Usuario ObtenerUsuarioPorCorreo(string correo)
        {
            Usuario usuario = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT IdUsuario, Nombre, Rol, Correo, IdGenero FROM Usuarios WHERE Correo = @Correo";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Correo", correo);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                IdUsuario = reader["IdUsuario"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Rol = reader["Rol"].ToString(),
                                Correo = reader["Correo"].ToString(),
                                IdGenero = reader["IdGenero"] != DBNull.Value ? Convert.ToInt32(reader["IdGenero"]) : null
                            };
                        }
                    }
                }
            }
            return usuario;
        }

        // 4. ACTUALIZAR PERFIL (Corregido para IdUsuario string)
        public bool ActualizarPerfilEstudiante(Usuario user)
        {
            if (!user.Correo.ToLower().EndsWith("@uisek.edu.ec")) return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Nota: No actualizamos IdUsuario porque es la PK (Cédula) y es fija.
                string sql = @"UPDATE Usuarios 
                               SET Nombre = @Nombre, 
                                   Correo = @Correo, 
                                   IdGenero = @IdGenero 
                               WHERE IdUsuario = @IdUsuario AND Rol = 'Estudiante'";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", user.Nombre);
                    cmd.Parameters.AddWithValue("@Correo", user.Correo);
                    cmd.Parameters.AddWithValue("@IdGenero", (object)user.IdGenero ?? DBNull.Value);

                    // Aquí IdUsuario es la Cédula que viene del modelo oculto en la vista
                    cmd.Parameters.AddWithValue("@IdUsuario", user.IdUsuario);

                    return cmd.ExecuteNonQuery() > 0;
                }
            }
        }

        // 5. DROPDOWN DE GÉNEROS
        public Dictionary<int, string> ObtenerGeneros()
        {
            var generos = new Dictionary<int, string>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT IdGenero, NombreGenero FROM Cat_Generos";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                        generos.Add((int)reader["IdGenero"], reader["NombreGenero"].ToString());
                }
            }
            return generos;
        }

        // 6. RECUPERAR CLAVE (Auxiliar)
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
                    if (result != null) clave = result.ToString();
                }
            }
            return clave;
        }


        // 7. CONTAR USUARIOS
        public int ContarUsuarios()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Contamos todos los registros de la tabla Usuarios
                string sql = "SELECT COUNT(*) FROM Usuarios";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    return (int)cmd.ExecuteScalar();
                }
            }
        }
    }
}