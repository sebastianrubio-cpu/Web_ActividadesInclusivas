using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Obtener la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// ENDPOINT: Buscar estudiante por Cédula
app.MapGet("/api/estudiantes/{cedula}", async (string cedula) =>
{
    using (var conn = new SqlConnection(connectionString))
    {
        await conn.OpenAsync();

        // 1. CORRECCIÓN CRÍTICA: 
        // - Buscamos en tabla 'Usuarios'
        // - Usamos 'IdUsuario' que ahora es la cédula
        // - Filtramos que el Rol sea 'Estudiante'
        var sql = @"SELECT TOP 1 IdUsuario, Nombre 
                    FROM Usuarios 
                    WHERE IdUsuario = @cedula AND Rol = 'Estudiante'";

        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@cedula", cedula);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    // 2. ÉXITO: Usuario encontrado
                    string nombre = reader["Nombre"].ToString();
                    string idEncontrado = reader["IdUsuario"].ToString();

                    return Results.Ok(new
                    {
                        cedula = idEncontrado,
                        esEstudiante = true,
                        nombre = nombre,
                        mensaje = "Estudiante activo encontrado."
                    });
                }
                else
                {
                    // 3. FALLO: No existe o no es estudiante
                    return Results.NotFound(new
                    {
                        cedula = cedula,
                        esEstudiante = false,
                        mensaje = "Cédula no encontrada o el usuario no tiene rol de Estudiante."
                    });
                }
            }
        }
    }
})
.WithName("GetEstudiante");

app.Run();