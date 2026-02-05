using Microsoft.AspNetCore.Authentication.Cookies;
using SistemaWeb.Models;
using SistemaWeb.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// 1. REGISTRAR SERVICIOS PROPIOS
builder.Services.AddHttpClient<EstudiantesClient>();
builder.Services.AddScoped<ActividadRepository>();
builder.Services.AddScoped<ActividadService>();
builder.Services.AddScoped<UsuarioRepository>();
builder.Services.AddSingleton<ILogService, LogService>();
builder.Services.AddScoped<CorreoService>();

// 2. CONFIGURAR SEGURIDAD (COOKIES)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(option => {
        option.LoginPath = "/Acceso/Login";
        option.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

// --- AGREGA ESTA LÍNEA PARA CORREGIR EL ERROR ---
builder.Services.AddAuthorization();
// -----------------------------------------------

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
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Acceso}/{action=Login}/{id?}");

app.Run();