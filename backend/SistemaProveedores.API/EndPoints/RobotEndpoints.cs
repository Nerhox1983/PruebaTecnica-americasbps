using Microsoft.EntityFrameworkCore;
using SistemaProveedores.API.Infrastructure;
using SistemaProveedores.Domain;

namespace SistemaProveedores.API.Endpoints;

public static class RobotEndpoints
{
    
    private static string? _archivoPendienteRuta = null;
    private static string? _archivoPendienteNombre = null;

    /// <summary>
    /// Registra los endpoints relacionados con la carga de archivos y la integración del robot procesador.
    /// Permite subir archivos, consultar pendientes y realizar inserciones masivas en la base de datos.
    /// </summary>
    /// <param name="app">El constructor de rutas de la aplicación.</param>
    /// <param name="uploadsFolder">Ruta física en el servidor donde se almacenarán temporalmente los archivos subidos.</param>
    public static void MapRobotEndpoints(this IEndpointRouteBuilder app, string uploadsFolder)
    {        
        app.MapPost("/api/proveedores/upload-archivo", async (HttpRequest request) =>
        {
            if (!request.HasFormContentType || !request.Form.Files.Any())
            {
                return Results.BadRequest(new { mensaje = "No se ha enviado ningún archivo válido." });
            }

            var file = request.Form.Files.First();
            string uniqueName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            string targetPath = Path.Combine(uploadsFolder, uniqueName);

            using (var stream = new FileStream(targetPath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _archivoPendienteRuta = targetPath;
            _archivoPendienteNombre = file.FileName;

            return Results.Ok(new { mensaje = "Archivo recibido en el servidor web.", archivoOriginal = file.FileName });
        });

        var robotGroup = app.MapGroup("/api/robot");

        robotGroup.MapGet("/archivo-pendiente", () =>
        {
            if (string.IsNullOrEmpty(_archivoPendienteRuta) || !File.Exists(_archivoPendienteRuta))
            {
                return Results.Ok(new { hayPendientes = false });
            }

            return Results.Ok(new
            {
                hayPendientes = true,
                nombreArchivo = _archivoPendienteNombre,
                contenido = File.ReadAllText(_archivoPendienteRuta)
            });
        });

        robotGroup.MapPost("/insertar-masivo", async (List<Proveedor> loteProveedores, AppDbContext db) =>
        {
            if (loteProveedores == null || !loteProveedores.Any()) return Results.BadRequest(new { mensaje = "El lote está vacío." });

            int insertados = 0;
            foreach (var prov in loteProveedores)
            {
                var existe = await db.Proveedores.AnyAsync(p => p.NitEmpresa == prov.NitEmpresa);
                if (!existe)
                {
                    prov.TipoCarga = "AUTOMATICO";
                    prov.EstadoRegistro = prov.EstadoRegistro.ToUpper().Trim();
                    db.Proveedores.Add(prov);
                    insertados++;
                }
            }

            if (insertados > 0) await db.SaveChangesAsync();

            if (!string.IsNullOrEmpty(_archivoPendienteRuta) && File.Exists(_archivoPendienteRuta))
            {
                File.Delete(_archivoPendienteRuta);
            }
            _archivoPendienteRuta = null;
            _archivoPendienteNombre = null;

            return Results.Ok(new { mensaje = "Procesamiento masivo completado.", registrosInsertados = insertados });
        });
    }
}
