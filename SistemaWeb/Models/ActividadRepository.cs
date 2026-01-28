using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace SistemaWeb.Models
{
    public class ActividadRepository
    {
        private readonly string? _connectionString;

        public ActividadRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Método usando STORED PROCEDURE
        public List<Actividad> ObtenerTodas()
        {
            var actividades = new List<Actividad>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                
                using (var command = new SqlCommand("sp_ObtenerActividades", connection))
                {
                    command.CommandType = CommandType.StoredProcedure; 

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            actividades.Add(new Actividad
                            {
                                Codigo = reader["Codigo"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                FechaRealizacion = (DateTime)reader["FechaRealizacion"],
                                TipoDiscapacidad = reader["TipoDiscapacidad"].ToString(), 
                                Cupo = (int)reader["Cupo"],
                                Responsable = reader["Responsable"].ToString(),
                                Estado = reader["Estado"].ToString(), 
                                GmailProfesor = reader["GmailProfesor"].ToString()
                            });
                        }
                    }
                }
            }
            return actividades;
        }

        public void Agregar(Actividad actividad)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("sp_InsertarActividad", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Pasamos parámetros. Ojo: El SP maneja internamente la conversión de Estado/Discapacidad a IDs
                    command.Parameters.AddWithValue("@Codigo", actividad.Codigo);
                    command.Parameters.AddWithValue("@Nombre", actividad.Nombre);
                    command.Parameters.AddWithValue("@FechaRealizacion", actividad.FechaRealizacion);
                    command.Parameters.AddWithValue("@Cupo", actividad.Cupo);
                    // Manejo de nulos en C# antes de enviar, porque la DB ya no los acepta
                    command.Parameters.AddWithValue("@Responsable", actividad.Responsable ?? "Sin Asignar");
                    command.Parameters.AddWithValue("@GmailProfesor", actividad.GmailProfesor ?? "sin_correo@uisek.edu.ec");
                    command.Parameters.AddWithValue("@NombreEstado", actividad.Estado ?? "Activo");
                    command.Parameters.AddWithValue("@NombreDiscapacidad", actividad.TipoDiscapacidad ?? "Ninguna");

                    command.ExecuteNonQuery();
                }
            }
        }
        public Actividad ObtenerPorId(string id)
        {
            Actividad actividad = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("sp_ObtenerActividadPorId", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Codigo", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            actividad = new Actividad
                            {
                                Codigo = reader["Codigo"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                FechaRealizacion = (DateTime)reader["FechaRealizacion"],
                                TipoDiscapacidad = reader["TipoDiscapacidad"].ToString(),
                                Cupo = (int)reader["Cupo"],
                                Responsable = reader["Responsable"].ToString(),
                                Estado = reader["Estado"].ToString(),
                                GmailProfesor = reader["GmailProfesor"].ToString()
                            };
                        }
                    }
                }
            }
            return actividad;
        }

        // Método: Actualizar
        public void Actualizar(Actividad actividad)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("sp_ActualizarActividad", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.AddWithValue("@Codigo", actividad.Codigo);
                    command.Parameters.AddWithValue("@Nombre", actividad.Nombre);
                    command.Parameters.AddWithValue("@FechaRealizacion", actividad.FechaRealizacion);
                    command.Parameters.AddWithValue("@Cupo", actividad.Cupo);
                    command.Parameters.AddWithValue("@Responsable", actividad.Responsable ?? "Sin Asignar");
                    command.Parameters.AddWithValue("@GmailProfesor", actividad.GmailProfesor ?? "sin_correo@uisek.edu.ec");
                    command.Parameters.AddWithValue("@NombreEstado", actividad.Estado ?? "Activo");
                    command.Parameters.AddWithValue("@NombreDiscapacidad", actividad.TipoDiscapacidad ?? "Ninguna");

                    command.ExecuteNonQuery();
                }
            }
        }

        // Método: Eliminar
        public void Eliminar(string codigo)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("sp_EliminarActividad", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@Codigo", codigo);
                    command.ExecuteNonQuery();
                }
            }
        }

    }
}