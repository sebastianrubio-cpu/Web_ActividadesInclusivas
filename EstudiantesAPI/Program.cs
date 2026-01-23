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
                        esEstudiante = reader.GetBoolean(0), 
                        nombre = reader.GetString(1),       
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
.WithName("GetEstudiante"); 

app.Run();