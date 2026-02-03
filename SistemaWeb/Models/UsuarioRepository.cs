using Microsoft.Data.SqlClient;
using System.Data;
using Microsoft.Extensions.Configuration; // Necesario para IConfiguration
using System.Collections.Generic; // Necesario para Dictionary
using System;

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

        // 2. REGISTRAR (Método genérico que llama el Controlador) [NUEVO]
        public bool Registrar(Usuario user)
        {
            // Llamamos a la lógica interna pasando los datos del objeto
            // Si IdGenero es null, pasamos 0 o DBNull según tu lógica (aquí asumo 0 por defecto)
            return RegistrarEstudiante(user.IdUsuario, user.Nombre, user.Correo, user.Clave, user.IdGenero ?? 0);
        }

        // 2.1 Lógica SQL de Registro
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

        // 3. OBTENER POR CORREO
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

        // 4. ACTUALIZAR PERFIL
        public bool ActualizarPerfilEstudiante(Usuario user)
        {
            if (!user.Correo.ToLower().EndsWith("@uisek.edu.ec")) return false;

            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
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

        // 6. CONTAR USUARIOS (Necesario para Estadísticas) [NUEVO]
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

        // 7. RECUPERAR CLAVE
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
    }
}