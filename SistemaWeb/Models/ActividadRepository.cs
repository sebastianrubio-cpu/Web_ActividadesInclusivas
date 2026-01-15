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

        public List<Actividad> ObtenerTodas()
        {
            var actividades = new List<Actividad>();

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT Codigo, Nombre, FechaRealizacion, TipoDiscapacidad, Cupo, Responsable, Estado FROM Actividades", connection))
                {
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
                                Estado = reader["Estado"].ToString()
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
                using (var command = new SqlCommand("INSERT INTO Actividades (Codigo, Nombre, FechaRealizacion, TipoDiscapacidad, Cupo, Responsable, Estado) VALUES (@Codigo, @Nombre, @FechaRealizacion, @TipoDiscapacidad, @Cupo, @Responsable, @Estado)", connection))
                {
                    command.Parameters.AddWithValue("@Codigo", actividad.Codigo);
                    command.Parameters.AddWithValue("@Nombre", actividad.Nombre);
                    command.Parameters.AddWithValue("@FechaRealizacion", actividad.FechaRealizacion);
                    command.Parameters.AddWithValue("@TipoDiscapacidad", actividad.TipoDiscapacidad ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Cupo", actividad.Cupo);
                    command.Parameters.AddWithValue("@Responsable", actividad.Responsable ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Estado", actividad.Estado ?? "Activo");
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
                using (var command = new SqlCommand("SELECT * FROM Actividades WHERE Codigo = @Codigo", connection))
                {
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
                                Estado = reader["Estado"].ToString()
                            };
                        }
                    }
                }
            }
            return actividad;
        }

        public void Actualizar(Actividad actividad)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("UPDATE Actividades SET Nombre=@Nombre, FechaRealizacion=@FechaRealizacion, TipoDiscapacidad=@TipoDiscapacidad, Cupo=@Cupo, Responsable=@Responsable, Estado=@Estado WHERE Codigo=@Codigo", connection))
                {
                    command.Parameters.AddWithValue("@Codigo", actividad.Codigo);
                    command.Parameters.AddWithValue("@Nombre", actividad.Nombre);
                    command.Parameters.AddWithValue("@FechaRealizacion", actividad.FechaRealizacion);
                    command.Parameters.AddWithValue("@TipoDiscapacidad", actividad.TipoDiscapacidad ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Cupo", actividad.Cupo);
                    command.Parameters.AddWithValue("@Responsable", actividad.Responsable ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Estado", actividad.Estado ?? "Activo");
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}