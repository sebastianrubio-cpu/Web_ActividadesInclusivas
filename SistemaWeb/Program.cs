var builder = WebApplication.CreateBuilder(args);

// 1. REGISTRO DE SERVICIOS
// -----------------------

// Repositorio de Datos (SQL)
builder.Services.AddScoped<SistemaWeb.Models.ActividadRepository>();

// Cliente API (Microservicio)
builder.Services.AddHttpClient<SistemaWeb.Services.EstudiantesClient>();

// Servicios de MVC (Controladores y Vistas)
builder.Services.AddControllersWithViews();


builder.Services.AddAuthorization();

builder.Services.AddScoped<SistemaWeb.Services.ActividadService>();
builder.Services.AddSingleton<SistemaWeb.Services.ILogService, SistemaWeb.Services.LogService>();

var app = builder.Build();

// 2. CONFIGURACIÓN DEL PIPELINE HTTP
// ----------------------------------

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Ahora esto funcionará porque ya agregamos AddAuthorization() arriba
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Actividades}/{action=Index}/{id?}");

app.Run();