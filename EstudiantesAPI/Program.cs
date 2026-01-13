using Microsoft.Data.SqlClient;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Agregar servicios básicos
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configurar Swagger (para probar la API)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// --- AQUÍ ESTÁ TU MICROSERVICIO ---

// Obtener la cadena de conexión
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Crear el Endpoint: GET /api/estudiantes/{cedula}
app.MapGet("/api/estudiantes/{cedula}", async (string cedula) =>
{
    using (var conn = new SqlConnection(connectionString))
    {
        await conn.OpenAsync();

        // Consulta SQL directa (Como en el PDF)
        var sql = "SELECT TOP 1 EsEstudiante, Nombre FROM Estudiantes WHERE Cedula = @cedula";

        using (var cmd = new SqlCommand(sql, conn))
        {
            cmd.Parameters.AddWithValue("@cedula", cedula);

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                if (await reader.ReadAsync())
                {
                    // Si encontramos la cédula
                    return Results.Ok(new
                    {
                        cedula = cedula,
                        esEstudiante = reader.GetBoolean(0), // Columna EsEstudiante
                        nombre = reader.GetString(1),        // Columna Nombre
                        mensaje = "Pertenece a un estudiante"
                    });
                }
                else
                {
                    // Si NO encontramos la cédula
                    return Results.NotFound(new
                    {
                        cedula = cedula,
                        mensaje = "Cédula no encontrada en UISEK"
                    });
                }
            }
        }
    }
})
.WithName("GetEstudiante"); // <--- Aquí terminamos, borramos el .WithOpenApi()

app.Run();