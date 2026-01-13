var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// --- AGREGA ESTA LÍNEA AQUÍ ---
builder.Services.AddHttpClient<SistemaWeb.Services.EstudiantesClient>(); 
// ------------------------------

builder.Services.AddScoped<SistemaWeb.Models.ActividadRepository>();

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
    pattern: "{controller=Actividades}/{action=Index}/{id?}");

app.Run();
