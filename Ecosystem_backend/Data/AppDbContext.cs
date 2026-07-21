using Ecosystem_backend.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Ecosystem_backend.Data
{
    public class AppDbContext: DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // DbSets para consultar y guardar datos
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Empleado> Empleados { get; set; }
        public DbSet<Prospecto> Prospectos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Cotizacion> Cotizaciones { get; set; }
        public DbSet<DetalleCotizacion> DetallesCotizaciones { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<OrdenServicio> OrdenesServicio { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuramos explícitamente que la relación IdUsuario debe ser única 
            // en Clientes y Empleados para asegurar el 1:1

            modelBuilder.Entity<Empleado>()
                .HasIndex(e => e.IdUsuario)
                .IsUnique();
        }
    }
}
