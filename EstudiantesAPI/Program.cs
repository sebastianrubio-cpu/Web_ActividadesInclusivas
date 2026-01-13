using System.Text.Json;

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

// --- INICIO DE LA LÓGICA DE ENDPOINTS ---

app.MapGet("/api/estudiantes/{cedula}", async (string cedula) =>
{
    // 1. Ruta del archivo JSON
    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "estudiantes.json");

    if (!File.Exists(filePath))
        return Results.Problem("El archivo de datos no existe.");

    // 2. Leer JSON
    var jsonString = await File.ReadAllTextAsync(filePath);
    var estudiantes = JsonSerializer.Deserialize<List<Estudiante>>(jsonString, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

    // 3. Buscar en la lista (LINQ en memoria)
    var encontrado = estudiantes?.FirstOrDefault(e => e.Cedula == cedula);

    if (encontrado != null)
    {
        return Results.Ok(new
        {
            cedula = encontrado.Cedula,
            esEstudiante = encontrado.EsEstudiante,
            nombre = encontrado.Nombre,
            mensaje = "Verificado desde JSON"
        });
    }
    else
    {
        return Results.NotFound(new { cedula = cedula, mensaje = "No encontrado en la lista JSON" });
    }
})
.WithName("GetEstudiante");

app.Run();
// --- FIN DE LAS INSTRUCCIONES ---

// --- DEFINICIONES DE TIPOS (SIEMPRE AL FINAL) ---
// El 'record' debe ir AQUÍ, después de que todo el programa haya corrido
record Estudiante(string Cedula, string Nombre, bool EsEstudiante);