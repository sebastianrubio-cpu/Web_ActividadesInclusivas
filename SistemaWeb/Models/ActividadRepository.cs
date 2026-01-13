using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration; // Ojo a este
using System.Collections.Generic;

namespace SistemaWeb.Models
{
    public class ActividadRepository
    {
        private readonly string _connectionString;

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
                using (var command = new SqlCommand("SELECT * FROM Actividades", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            actividades.Add(new Actividad
                            {
                                Id = (int)reader["Id"],
                                Titulo = reader["Titulo"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                Fecha = (DateTime)reader["Fecha"],
                                Lugar = reader["Lugar"].ToString()
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
                using (var command = new SqlCommand("INSERT INTO Actividades (Titulo, Descripcion, Fecha, Lugar) VALUES (@Titulo, @Descripcion, @Fecha, @Lugar)", connection))
                {
                    command.Parameters.AddWithValue("@Titulo", actividad.Titulo);
                    command.Parameters.AddWithValue("@Descripcion", actividad.Descripcion);
                    command.Parameters.AddWithValue("@Fecha", actividad.Fecha);
                    command.Parameters.AddWithValue("@Lugar", actividad.Lugar);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Agregué el método de actualización básico para que funcione Editar
        public void Actualizar(Actividad actividad)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("UPDATE Actividades SET Titulo=@Titulo, Descripcion=@Descripcion, Fecha=@Fecha, Lugar=@Lugar WHERE Id=@Id", connection))
                {
                    command.Parameters.AddWithValue("@Titulo", actividad.Titulo);
                    command.Parameters.AddWithValue("@Descripcion", actividad.Descripcion);
                    command.Parameters.AddWithValue("@Fecha", actividad.Fecha);
                    command.Parameters.AddWithValue("@Lugar", actividad.Lugar);
                    command.Parameters.AddWithValue("@Id", actividad.Id);
                    command.ExecuteNonQuery();
                }
            }
        }

        // Agregué ObtenerPorId para que funcione Editar
        public Actividad ObtenerPorId(int id)
        {
            Actividad actividad = null;
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var command = new SqlCommand("SELECT * FROM Actividades WHERE Id = @Id", connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            actividad = new Actividad
                            {
                                Id = (int)reader["Id"],
                                Titulo = reader["Titulo"].ToString(),
                                Descripcion = reader["Descripcion"].ToString(),
                                Fecha = (DateTime)reader["Fecha"],
                                Lugar = reader["Lugar"].ToString()
                            };
                        }
                    }
                }
            }
            return actividad;
        }
    }
}