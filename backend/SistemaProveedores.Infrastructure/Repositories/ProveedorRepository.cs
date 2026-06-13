using Microsoft.EntityFrameworkCore;
using SistemaProveedores.API.Infrastructure;
using SistemaProveedores.Domain;
using SistemaProveedores.Domain.Interfaces;

namespace SistemaProveedores.Infrastructure.Repositories
{

    public class ProveedorRepository : IProveedorRepository
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Inicializa una nueva instancia del repositorio de proveedores con su contexto de base de datos.
        /// </summary>
        /// <param name="context">Contexto de Entity Framework para la persistencia.</param>
        public ProveedorRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtiene todos los proveedores registrados, ordenados por identificador de forma descendente.
        /// </summary>
        /// <returns>Una colección de entidades <see cref="Proveedor"/>.</returns>
        public async Task<IEnumerable<Proveedor>> ObtenerTodosAsync()
        {
            return await _context.Proveedores.OrderByDescending(p => p.Id).ToListAsync();
        }

        /// <summary>
        /// Busca un proveedor específico utilizando su identificador único.
        /// </summary>
        /// <param name="id">El ID del proveedor a buscar.</param>
        /// <returns>El proveedor encontrado o <c>null</c> si no existe.</returns>
        public async Task<Proveedor?> ObtenerPorIdAsync(int id)
        {
            return await _context.Proveedores.FindAsync(id);
        }

        /// <summary>
        /// Verifica la existencia de un proveedor en la base de datos basado en su NIT.
        /// </summary>
        /// <param name="nitEmpresa">NIT de la empresa a validar.</param>
        /// <returns><c>true</c> si el NIT ya existe; de lo contrario, <c>false</c>.</returns>
        public async Task<bool> ExisteNitAsync(string nitEmpresa)
        {
            return await _context.Proveedores.AnyAsync(p => p.NitEmpresa == nitEmpresa);
        }

        /// <summary>
        /// Valida si un NIT ya está asignado a un proveedor distinto al que se está editando.
        /// </summary>
        /// <param name="nitEmpresa">NIT a verificar.</param>
        /// <param name="idExcluido">ID del proveedor actual para excluirlo de la búsqueda.</param>
        /// <returns><c>true</c> si el NIT colisiona con otro registro; de lo contrario, <c>false</c>.</returns>
        public async Task<bool> ExisteNitDuplicadoAsync(string nitEmpresa, int idExcluido)
        {
            return await _context.Proveedores.AnyAsync(p => p.NitEmpresa == nitEmpresa && p.Id != idExcluido);
        }

        /// <summary>
        /// Agrega un nuevo proveedor al contexto y persiste los cambios de forma asíncrona.
        /// </summary>
        /// <param name="proveedor">La entidad proveedor a registrar.</param>
        public async Task InsertarManualAsync(Proveedor proveedor)
        {
            await _context.Proveedores.AddAsync(proveedor);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Guarda los cambios pendientes en el contexto de base de datos para las entidades modificadas.
        /// </summary>
        public async Task ActualizarAsync()
        {
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Realiza una inserción masiva de proveedores y guarda los cambios en una sola transacción.
        /// </summary>
        /// <param name="proveedores">Lista de proveedores a insertar.</param>
        public async Task InsertarMasivoAsync(IEnumerable<Proveedor> proveedores)
        {
            await _context.Proveedores.AddRangeAsync(proveedores);
            await _context.SaveChangesAsync();
        }
    }
}