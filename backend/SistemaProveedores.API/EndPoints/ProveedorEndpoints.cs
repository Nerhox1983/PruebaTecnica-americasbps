using Microsoft.EntityFrameworkCore;
using SistemaProveedores.API.Infrastructure;
using SistemaProveedores.Domain;

namespace SistemaProveedores.API.Endpoints;

public static class ProveedorEndpoints
{
    /// <summary>
    /// Define las rutas de API para la gestión manual de proveedores, incluyendo consultas, creación y edición.
    /// </summary>
    /// <param name="app">El constructor de rutas de la aplicación.</param>
    public static void MapProveedorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/proveedores");
        
        group.MapGet("/", async (AppDbContext db) =>
        {
            var lista = await db.Proveedores.OrderByDescending(p => p.Id).ToListAsync();
            return Results.Ok(lista);
        });

        group.MapPost("/manual", async (Proveedor proveedor, AppDbContext db) =>
        {
            var existeNit = await db.Proveedores.AnyAsync(p => p.NitEmpresa == proveedor.NitEmpresa);
            if (existeNit)
            {
                return Results.BadRequest(new { mensaje = $"El NIT {proveedor.NitEmpresa} ya está registrado." });
            }

            proveedor.TipoCarga = "MANUAL";
            proveedor.EstadoRegistro = proveedor.EstadoRegistro.ToUpper();

            db.Proveedores.Add(proveedor);
            await db.SaveChangesAsync();

            return Results.Created($"/api/proveedores/{proveedor.Id}", proveedor);
        });

        group.MapPut("/{id}", async (int id, Proveedor proveedorActualizado, AppDbContext db) =>
        {
            var proveedorExistente = await db.Proveedores.FindAsync(id);
            if (proveedorExistente == null) return Results.NotFound(new { mensaje = "Proveedor no encontrado." });

            if (proveedorExistente.NitEmpresa != proveedorActualizado.NitEmpresa)
            {
                var nitDuplicado = await db.Proveedores.AnyAsync(p => p.NitEmpresa == proveedorActualizado.NitEmpresa && p.Id != id);
                if (nitDuplicado) return Results.BadRequest(new { mensaje = "El nuevo NIT ya está en uso." });
            }

            proveedorExistente.NitEmpresa = proveedorActualizado.NitEmpresa.Trim();
            proveedorExistente.NombreEmpresa = proveedorActualizado.NombreEmpresa.Trim();
            proveedorExistente.Pais = proveedorActualizado.Pais.Trim();
            proveedorExistente.EstadoRegistro = proveedorActualizado.EstadoRegistro.ToUpper().Trim();

            await db.SaveChangesAsync();
            return Results.Ok(proveedorExistente);
        });
    }
}