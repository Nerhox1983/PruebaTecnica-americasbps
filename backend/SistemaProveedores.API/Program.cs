using Microsoft.EntityFrameworkCore;
using SistemaProveedores.API.Endpoints;
using SistemaProveedores.API.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var connectionString = builder.Configuration.GetConnectionString("PostgresConnection");
if (string.IsNullOrEmpty(connectionString)) throw new InvalidOperationException("Falta ConnectionString.");

builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));

var app = builder.Build();

app.UseCors();

string uploadsFolder = Path.Combine(app.Environment.ContentRootPath, "uploads");
if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

app.MapGet("/", () => Results.Ok(new { Plataforma = "Sistema API Activo", Servidor = DateTime.UtcNow }));
app.MapRobotEndpoints(uploadsFolder);

app.MapProveedorEndpoints();

app.Run();