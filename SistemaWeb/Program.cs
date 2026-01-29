
using Microsoft.AspNetCore.Authentication.Cookies; // <--- IMPORTANTE
using SistemaWeb.Models;
using SistemaWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. REGISTRAR SERVICIOS PROPIOS
builder.Services.AddHttpClient<EstudiantesClient>();
builder.Services.AddScoped<ActividadRepository>();
builder.Services.AddScoped<ActividadService>();
builder.Services.AddScoped<UsuarioRepository>(); // <--- NUEVO REPO
builder.Services.AddSingleton<ILogService, LogService>();
builder.Services.AddScoped<CorreoService>();

// 2. CONFIGURAR SEGURIDAD (COOKIES)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option => {
        option.LoginPath = "/Acceso/Login"; // Si no estás logueado, te manda aquí
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// 3. ACTIVAR SEGURIDAD
app.UseAuthentication(); // <--- IMPORTANTE: ¿Quién eres?
app.UseAuthorization();  // <--- IMPORTANTE: ¿Qué puedes hacer?

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}"); // Cambiamos para que arranque en Login

app.Run();