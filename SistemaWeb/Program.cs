var builder = WebApplication.CreateBuilder(args);
// Registrar el Repositorio de Actividades
builder.Services.AddScoped<SistemaWeb.Models.ActividadRepository>();

// Registrar el Cliente HTTP para la API de estudiantes
builder.Services.AddHttpClient<SistemaWeb.Services.EstudiantesClient>();

// Habilitar MVC (Controllers con Vistas)
builder.Services.AddControllersWithViews();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
