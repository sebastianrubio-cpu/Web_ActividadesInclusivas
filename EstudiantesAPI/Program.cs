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

app.MapGet("/api/estudiantes/{cedula}", async (string cedula) =>
{
    using (var conn = new SqlConnection(connectionString))
    {
        await conn.OpenAsync();

        // 1. CORREGIDO: Seleccionamos columnas que SÍ existen (Nombre, Apellido)
        var sql = "SELECT TOP 1 IdEstudiante, Nombre, Apellido FROM Estudiantes WHERE Cedula = @cedula";

        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@cedula", cedula);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    // 2. LOGICA: Si entró aquí, es porque encontró el registro.
                    // Armamos el nombre completo y devolvemos true "hardcodeado" porque la validación pasó.
                    string nombreCompleto = $"{reader["Nombre"]} {reader["Apellido"]}";

                    return Results.Ok(new
                    {
                        cedula = cedula,
                        esEstudiante = true, // Si lo encontró en la DB, es true.
                        nombre = nombreCompleto,
                        mensaje = "Pertenece a un estudiante"
                    });
                }
                else
                {
                    // Si NO encontra la cédula
                    return Results.NotFound(new
                    {
                        cedula = cedula,
                        esEstudiante = false,
                        mensaje = "Cédula no encontrada en UISEK"
                    });
                }
            }
        }
    }
})
.WithName("GetEstudiante");

app.Run();