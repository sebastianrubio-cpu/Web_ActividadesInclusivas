using System.Data;
using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using System;

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
            var lista = new List<Actividad>();
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("sp_ObtenerActividades", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Actividad
                            {
                                Codigo = reader["Codigo"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                FechaRealizacion = Convert.ToDateTime(reader["FechaRealizacion"]),
                                Cupo = Convert.ToInt32(reader["Cupo"]),
                                Responsable = reader["Responsable"].ToString(),
                                GmailProfesor = reader["GmailProfesor"] != DBNull.Value ? reader["GmailProfesor"].ToString() : "",
                                Estado = reader["Estado"].ToString(),
                                TipoDiscapacidad = reader["TipoDiscapacidad"].ToString(),
                                Latitud = reader["Latitud"] != DBNull.Value ? Convert.ToDouble(reader["Latitud"]) : 0,
                                Longitud = reader["Longitud"] != DBNull.Value ? Convert.ToDouble(reader["Longitud"]) : 0
                            });
                        }
                    }
                }
            }
            return lista;
        }

        public Actividad ObtenerPorId(string codigo)
        {
            Actividad actividad = null;
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("sp_ObtenerActividadPorId", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Codigo", codigo);

                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            actividad = new Actividad
                            {
                                Codigo = reader["Codigo"].ToString(),
                                Nombre = reader["Nombre"].ToString(),
                                FechaRealizacion = Convert.ToDateTime(reader["FechaRealizacion"]),
                                Cupo = Convert.ToInt32(reader["Cupo"]),
                                Responsable = reader["Responsable"].ToString(),
                                GmailProfesor = reader["GmailProfesor"] != DBNull.Value ? reader["GmailProfesor"].ToString() : "",
                                Estado = reader["Estado"].ToString(),
                                TipoDiscapacidad = reader["TipoDiscapacidad"].ToString(),
                                Latitud = reader["Latitud"] != DBNull.Value ? Convert.ToDouble(reader["Latitud"]) : 0,
                                Longitud = reader["Longitud"] != DBNull.Value ? Convert.ToDouble(reader["Longitud"]) : 0
                            };
                        }
                    }
                }
            }
            return actividad;
        }

        public void Agregar(Actividad actividad, string idUsuarioAuditoria)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("sp_InsertarActividad", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Codigo", actividad.Codigo);
                    cmd.Parameters.AddWithValue("@Nombre", actividad.Nombre);
                    cmd.Parameters.AddWithValue("@FechaRealizacion", actividad.FechaRealizacion);
                    cmd.Parameters.AddWithValue("@Cupo", actividad.Cupo);
                    cmd.Parameters.AddWithValue("@IdResponsable", actividad.IdResponsable);
                    cmd.Parameters.AddWithValue("@Latitud", actividad.Latitud);
                    cmd.Parameters.AddWithValue("@Longitud", actividad.Longitud);
                    cmd.Parameters.AddWithValue("@NombreEstado", actividad.Estado ?? "Activo");
                    cmd.Parameters.AddWithValue("@NombreDiscapacidad", actividad.TipoDiscapacidad ?? "Ninguna");

                    // Este parámetro viaja al SP y se inyecta en la Sesión para el Trigger
                    cmd.Parameters.AddWithValue("@IdUsuarioAuditoria", idUsuarioAuditoria);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Actualizar(Actividad actividad, string idUsuarioAuditoria)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("sp_ActualizarActividad", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Codigo", actividad.Codigo);
                    cmd.Parameters.AddWithValue("@Nombre", actividad.Nombre);
                    cmd.Parameters.AddWithValue("@FechaRealizacion", actividad.FechaRealizacion);
                    cmd.Parameters.AddWithValue("@Cupo", actividad.Cupo);
                    cmd.Parameters.AddWithValue("@IdResponsable", actividad.IdResponsable);
                    cmd.Parameters.AddWithValue("@Latitud", actividad.Latitud);
                    cmd.Parameters.AddWithValue("@Longitud", actividad.Longitud);
                    cmd.Parameters.AddWithValue("@NombreEstado", actividad.Estado ?? "Activo");
                    cmd.Parameters.AddWithValue("@NombreDiscapacidad", actividad.TipoDiscapacidad ?? "Ninguna");
                    cmd.Parameters.AddWithValue("@IdUsuarioAuditoria", idUsuarioAuditoria);

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void Eliminar(string codigo, string idUsuarioAuditoria)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("sp_EliminarActividad", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@Codigo", codigo);
                    cmd.Parameters.AddWithValue("@IdUsuarioAuditoria", idUsuarioAuditoria);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public dynamic ObtenerEstadisticasGlobales()
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                var totalUsuarios = new SqlCommand("SELECT COUNT(*) FROM Usuarios", conn).ExecuteScalar();
                var totalActividades = new SqlCommand("SELECT COUNT(*) FROM Actividades", conn).ExecuteScalar();
                return new { Usuarios = totalUsuarios, Actividades = totalActividades };
            }
        }
    }
}