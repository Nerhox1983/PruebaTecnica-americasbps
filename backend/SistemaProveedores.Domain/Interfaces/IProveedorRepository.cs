namespace SistemaProveedores.Domain.Interfaces
{
    public interface IProveedorRepository
    {
        Task<IEnumerable<Proveedor>> ObtenerTodosAsync();
        Task<Proveedor?> ObtenerPorIdAsync(int id);
        Task<bool> ExisteNitAsync(string nitEmpresa);
        Task<bool> ExisteNitDuplicadoAsync(string nitEmpresa, int idExcluido);
        Task InsertarManualAsync(Proveedor proveedor);
        Task InsertarMasivoAsync(IEnumerable<Proveedor> proveedores);
        Task ActualizarAsync();
    }
}