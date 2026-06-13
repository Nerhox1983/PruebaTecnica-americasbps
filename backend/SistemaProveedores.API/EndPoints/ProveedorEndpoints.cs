using SistemaProveedores.Domain;
using SistemaProveedores.Domain.Interfaces;

namespace SistemaProveedores.API.Endpoints;

public static class ProveedorEndpoints
{
    /// <summary>
    /// Define las rutas de API para la gestión manual de proveedores, incluyendo consultas, creación y edición desacopladas.
    /// </summary>
    /// <param name="app">El constructor de rutas de la aplicación.</param>
    public static void MapProveedorEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/proveedores");

        // GET: /api/proveedores
        group.MapGet("/", async (IProveedorRepository repo) =>
        {
            var lista = await repo.ObtenerTodosAsync();
            return Results.Ok(lista);
        });

        // POST: /api/proveedores/manual
        group.MapPost("/manual", async (Proveedor proveedor, IProveedorRepository repo) =>
        {
            var existeNit = await repo.ExisteNitAsync(proveedor.NitEmpresa);
            if (existeNit)
            {
                return Results.BadRequest(new { mensaje = $"El NIT {proveedor.NitEmpresa} ya está registrado." });
            }

            // Aplicar lógica y normalización nativa del dominio
            proveedor.TipoCarga = "MANUAL";
            proveedor.EstadoRegistro = proveedor.EstadoRegistro.ToUpper().Trim();
            proveedor.NormalizarCampos();

            // Validación estructural antes de persistir
            var (esValido, mensajeError) = proveedor.ValidarEstructura();
            if (!esValido) return Results.BadRequest(new { mensaje = mensajeError });

            await repo.InsertarManualAsync(proveedor);

            return Results.Created($"/api/proveedores/{proveedor.Id}", proveedor);
        });

        // PUT: /api/proveedores/{id}
        group.MapPut("/{id}", async (int id, Proveedor proveedorActualizado, IProveedorRepository repo) =>
        {
            var proveedorExistente = await repo.ObtenerPorIdAsync(id);
            if (proveedorExistente == null) return Results.NotFound(new { mensaje = "Proveedor no encontrado." });

            // Si intenta cambiar el NIT, validamos que no colisione con otra empresa existente
            if (proveedorExistente.NitEmpresa != proveedorActualizado.NitEmpresa)
            {
                var nitDuplicado = await repo.ExisteNitDuplicadoAsync(proveedorActualizado.NitEmpresa, id);
                if (nitDuplicado) return Results.BadRequest(new { mensaje = "El nuevo NIT ya está en uso por otra empresa." });
            }

            // Mapeo y saneamiento de campos utilizando las bondades del Change Tracking
            proveedorExistente.NitEmpresa = proveedorActualizado.NitEmpresa.Trim();
            proveedorExistente.NombreEmpresa = proveedorActualizado.NombreEmpresa.Trim();
            proveedorExistente.Pais = proveedorActualizado.Pais.Trim();
            proveedorExistente.EstadoRegistro = proveedorActualizado.EstadoRegistro.ToUpper().Trim();

            proveedorExistente.NormalizarCampos();

            await repo.ActualizarAsync();
            return Results.Ok(proveedorExistente);
        });
    }
}