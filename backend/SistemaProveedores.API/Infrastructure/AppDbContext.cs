using Microsoft.EntityFrameworkCore;
using SistemaProveedores.Domain;

namespace SistemaProveedores.API.Infrastructure
{
    public class AppDbContext : DbContext
    {
        /// <summary>
        /// Inicializa una nueva instancia del contexto de base de datos para el sistema de proveedores.
        /// </summary>
        /// <param name="options">Las opciones de configuración para este DbContext (ConnectionString, etc.).</param>
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        
        /// <summary>
        /// Representa la colección de proveedores en la base de datos.
        /// </summary>
        public DbSet<Proveedor> Proveedores => Set<Proveedor>();

        /// <summary>
        /// Configura el mapeo de las entidades hacia las tablas de la base de datos PostgreSQL, 
        /// definiendo nombres de tablas y columnas específicos.
        /// </summary>
        /// <param name="modelBuilder">El constructor de modelos de Entity Framework.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Proveedor>().ToTable("proveedores");

            modelBuilder.Entity<Proveedor>().Property(p => p.Id).HasColumnName("id");
            modelBuilder.Entity<Proveedor>().Property(p => p.NitEmpresa).HasColumnName("nit_empresa");
            modelBuilder.Entity<Proveedor>().Property(p => p.NombreEmpresa).HasColumnName("nombre_empresa");
            modelBuilder.Entity<Proveedor>().Property(p => p.Pais).HasColumnName("pais");
            modelBuilder.Entity<Proveedor>().Property(p => p.TipoCarga).HasColumnName("tipo_carga");
            modelBuilder.Entity<Proveedor>().Property(p => p.EstadoRegistro).HasColumnName("estado_registro");
        }
    }
}