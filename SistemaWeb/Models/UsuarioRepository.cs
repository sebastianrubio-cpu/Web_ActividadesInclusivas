using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration; // Necesario para IConfiguration
using System;
using System.Collections.Generic; // Necesario para Dictionary
using System.ComponentModel.DataAnnotations;
using System.Data;

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

        public int IdRol { get; set; } // Para guardar en BD (FK)
        public string? Rol { get; set; } // Para leer el nombre (JOIN)

        public int? IdGenero { get; set; }
    }

    public class UsuarioRepository
    {
        private readonly string _connectionString;

        public UsuarioRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // 1. VALIDAR LOGIN (Con JOIN a Cat_Roles)
        public Usuario ValidarUsuario(string correo, string clave)
        {
            Usuario usuario = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // JOIN clave para obtener el NombreRol a partir del IdRol
                string sql = @"
                    SELECT U.IdUsuario, U.Nombre, U.Correo, U.IdGenero, U.IdRol, R.NombreRol 
                    FROM Usuarios U
                    INNER JOIN Cat_Roles R ON U.IdRol = R.IdRol
                    WHERE U.Correo = @Correo AND U.Clave = @Clave";

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
                                Correo = reader["Correo"].ToString(),
                                IdRol = Convert.ToInt32(reader["IdRol"]),
                                Rol = reader["NombreRol"].ToString(), // Mapeamos el nombre aquí
                                IdGenero = reader["IdGenero"] != DBNull.Value ? Convert.ToInt32(reader["IdGenero"]) : null
                            };
                        }
                    }
                }
            }
            return usuario;
        }

        // 2. REGISTRAR (Método genérico)
        public bool Registrar(Usuario user)
        {
            return RegistrarEstudiante(user.IdUsuario, user.Nombre, user.Correo, user.Clave, user.IdGenero ?? 0);
        }

        // 2.1 Lógica SQL de Registro (Usa IdRol = 3 para Estudiantes)
        public bool RegistrarEstudiante(string cedula, string nombre, string correo, string clave, int idGenero)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    // Hardcodeamos el 3 porque en el Seed Data, 3 es Estudiante
                    string sql = @"INSERT INTO Usuarios (IdUsuario, Nombre, Correo, Clave, IdRol, IdGenero) 
                                   VALUES (@IdUsuario, @Nombre, @Correo, @Clave, 3, @IdGenero)";

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
                return false;
            }
        }

        // 3. OBTENER POR CORREO (Para validaciones o edición)
        public Usuario ObtenerUsuarioPorCorreo(string correo)
        {
            Usuario usuario = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT U.IdUsuario, U.Nombre, U.Correo, U.IdGenero, U.IdRol, R.NombreRol 
                    FROM Usuarios U
                    INNER JOIN Cat_Roles R ON U.IdRol = R.IdRol
                    WHERE U.Correo = @Correo";

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
                                IdRol = Convert.ToInt32(reader["IdRol"]),
                                Rol = reader["NombreRol"].ToString(),
                                Correo = reader["Correo"].ToString(),
                                IdGenero = reader["IdGenero"] != DBNull.Value ? Convert.ToInt32(reader["IdGenero"]) : null
                            };
                        }
                    }
                }
            }
            return usuario;
        }

        // 4. ACTUALIZAR PERFIL
        public bool ActualizarPerfilEstudiante(Usuario user)
        {
            if (!user.Correo.ToLower().EndsWith("@uisek.edu.ec")) return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // Validamos que sea Rol 3 (Estudiante) antes de actualizar
                string sql = @"UPDATE Usuarios 
                               SET Nombre = @Nombre, 
                                   Correo = @Correo, 
                                   IdGenero = @IdGenero 
                               WHERE IdUsuario = @IdUsuario AND IdRol = 3";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Nombre", user.Nombre);
                    cmd.Parameters.AddWithValue("@Correo", user.Correo);
                    cmd.Parameters.AddWithValue("@IdGenero", (object)user.IdGenero ?? DBNull.Value);
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

        // 6. CONTAR USUARIOS
        public int ContarUsuarios()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT COUNT(*) FROM Usuarios";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    var result = cmd.ExecuteScalar();
                    return result != DBNull.Value ? Convert.ToInt32(result) : 0;
                }
            }
        }

        // 7. OBTENER PROFESORES (Para el Dropdown de Actividades)
        public List<Usuario> ObtenerProfesores()
        {
            var lista = new List<Usuario>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                // IdRol 2 = Profesor según Seed Data
                string sql = "SELECT IdUsuario, Nombre, Correo FROM Usuarios WHERE IdRol = 2";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Usuario
                        {
                            IdUsuario = reader["IdUsuario"].ToString(),
                            Nombre = reader["Nombre"].ToString(),
                            Correo = reader["Correo"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        // 8. OBTENER TODOS LOS USUARIOS 
        public List<Usuario> ObtenerTodos()
        {
            var lista = new List<Usuario>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"
                    SELECT U.IdUsuario, U.Nombre, U.Correo, U.IdRol, R.NombreRol, U.IdGenero 
                    FROM Usuarios U
                    INNER JOIN Cat_Roles R ON U.IdRol = R.IdRol";

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new Usuario
                        {
                            IdUsuario = reader["IdUsuario"].ToString(),
                            Nombre = reader["Nombre"].ToString(),
                            Correo = reader["Correo"].ToString(),
                            IdRol = Convert.ToInt32(reader["IdRol"]),
                            Rol = reader["NombreRol"].ToString(), // Nombre del Rol
                            IdGenero = reader["IdGenero"] != DBNull.Value ? Convert.ToInt32(reader["IdGenero"]) : null
                        });
                    }
                }
            }
            return lista;
        }

        // 9. OBTENER LISTA DE ROLES 
        public List<dynamic> ObtenerListaRoles()
        {
            var lista = new List<dynamic>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = "SELECT IdRol, NombreRol FROM Cat_Roles";
                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        lista.Add(new
                        {
                            IdRol = (int)reader["IdRol"],
                            NombreRol = reader["NombreRol"].ToString()
                        });
                    }
                }
            }
            return lista;
        }

        // 10. CREAR USUARIO ADMIN
        public bool CrearUsuarioConRol(Usuario user)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"INSERT INTO Usuarios (IdUsuario, Nombre, Correo, Clave, IdRol, IdGenero) 
                                   VALUES (@IdUsuario, @Nombre, @Correo, @Clave, @IdRol, @IdGenero)";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", user.IdUsuario);
                        cmd.Parameters.AddWithValue("@Nombre", user.Nombre);
                        cmd.Parameters.AddWithValue("@Correo", user.Correo);
                        cmd.Parameters.AddWithValue("@Clave", user.Clave);
                        cmd.Parameters.AddWithValue("@IdRol", user.IdRol); // Rol seleccionado
                        cmd.Parameters.AddWithValue("@IdGenero", (object)user.IdGenero ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
        // 11. OBTENER USUARIO POR CÉDULA 
        public Usuario ObtenerPorId(string idUsuario)
        {
            Usuario usuario = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                string sql = @"SELECT * FROM Usuarios WHERE IdUsuario = @IdUsuario";
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            usuario = new Usuario
                            {
                                IdUsuario = reader["IdUsuario"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                Correo = reader["Correo"].ToString(),
                                Clave = reader["Clave"].ToString(), // Necesaria para el modelo
                                IdRol = Convert.ToInt32(reader["IdRol"]),
                                IdGenero = reader["IdGenero"] != DBNull.Value ? Convert.ToInt32(reader["IdGenero"]) : null
                            };
                        }
                    }
                }
            }
            return usuario;
        }

        // 12. ACTUALIZAR USUARIO 
        public bool Actualizar(Usuario user)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    string sql = @"UPDATE Usuarios 
                                   SET Nombre = @Nombre, 
                                       Correo = @Correo, 
                                       Clave = @Clave, 
                                       IdRol = @IdRol, 
                                       IdGenero = @IdGenero 
                                   WHERE IdUsuario = @IdUsuario";

                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Nombre", user.Nombre);
                        cmd.Parameters.AddWithValue("@Correo", user.Correo);
                        cmd.Parameters.AddWithValue("@Clave", user.Clave);
                        cmd.Parameters.AddWithValue("@IdRol", user.IdRol);
                        cmd.Parameters.AddWithValue("@IdGenero", (object)user.IdGenero ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@IdUsuario", user.IdUsuario);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch { return false; }
        }

        // 13. ELIMINAR USUARIO
        public bool Eliminar(string idUsuario)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                   
                    string sql = "DELETE FROM Usuarios WHERE IdUsuario = @IdUsuario";
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@IdUsuario", idUsuario);
                        cmd.ExecuteNonQuery();
                    }
                }
                return true;
            }
            catch { return false; }
        }
    }
}