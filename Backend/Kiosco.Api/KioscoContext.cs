using Microsoft.EntityFrameworkCore;
using Kiosco.Api.Models;

namespace Kiosco.Api
{
    public class KioscoContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }
        public DbSet<CierreCaja> CierresCaja { get; set; } 

        public KioscoContext(DbContextOptions<KioscoContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("usuarios");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
                entity.Property(e => e.PasswordHash).HasColumnName("password_hash");
                entity.Property(e => e.Rol).HasColumnName("rol");
            });

            modelBuilder.Entity<Categoria>(entity =>
            {
                entity.ToTable("categorias");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
            });

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("productos");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
                entity.Property(e => e.CategoriaId).HasColumnName("id_categoria");
                entity.Property(e => e.CodigoBarras).HasColumnName("codigo_barras");
                entity.Property(e => e.Stock).HasColumnName("stock");
                entity.Property(e => e.PrecioCosto)
                    .HasColumnName("precio_costo")
                    .HasPrecision(12, 2);
                entity.Property(e => e.PrecioVenta)
                    .HasColumnName("precio_venta")
                    .HasPrecision(12, 2);

                // Si borran la categoria, el producto queda con id_categoria en NULL
                entity.HasOne(e => e.Categoria)
                    .WithMany()
                    .HasForeignKey(e => e.CategoriaId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<Cliente>(entity =>
            {
                entity.ToTable("clientes");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
                entity.Property(e => e.DniCuit).HasColumnName("dni_cuit");
                entity.Property(e => e.RazonSocial).HasColumnName("razon_social");
                entity.Property(e => e.Domicilio).HasColumnName("domicilio");
            });

            modelBuilder.Entity<Venta>(entity =>
            {
                entity.ToTable("ventas");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Fecha).HasColumnName("fecha");
                entity.Property(e => e.MedioPago).HasColumnName("medio_pago");
                entity.Property(e => e.Total)
                    .HasColumnName("total")
                    .HasPrecision(12, 2);
                entity.Property(e => e.MontoRecibido)
                    .HasColumnName("monto_recibido")
                    .HasPrecision(12, 2);
                entity.Property(e => e.Vuelto)
                    .HasColumnName("vuelto")
                    .HasPrecision(12, 2);
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
                entity.Property(e => e.IdCliente).HasColumnName("id_cliente");
                
                // Mapeo de id_cierre
                entity.Property(e => e.IdCierre).HasColumnName("id_cierre");

                // Relación Venta -> Usuario (RESTRICT por defecto en MySQL, no se borra usuario con ventas)
                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación Venta -> Cliente (RESTRICT, no se borra cliente con ventas)
                entity.HasOne(e => e.Cliente)
                    .WithMany(c => c.Ventas)
                    .HasForeignKey(e => e.IdCliente)
                    .OnDelete(DeleteBehavior.Restrict);

                // Relación Venta -> Cierre
                entity.HasOne(e => e.CierreCaja)
                    .WithMany(c => c.Ventas)
                    .HasForeignKey(e => e.IdCierre)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.ToTable("detalle_ventas");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.IdVenta).HasColumnName("id_venta");
                entity.Property(e => e.IdProducto).HasColumnName("id_producto");
                entity.Property(e => e.Cantidad).HasColumnName("cantidad");
                entity.Property(e => e.PrecioUnitario)
                    .HasColumnName("precio_unitario")
                    .HasPrecision(12, 2);
                entity.Property(e => e.Subtotal)
                    .HasColumnName("subtotal")
                    .HasPrecision(12, 2);

                // Relación Detalle -> Venta (CASCADE: si se borra la venta, se borran sus ítems)
                entity.HasOne(e => e.Venta)
                    .WithMany(v => v.Detalle)
                    .HasForeignKey(e => e.IdVenta)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación Detalle -> Producto (RESTRICT: no se borra producto si fue vendido)
                entity.HasOne(e => e.Producto)
                    .WithMany()
                    .HasForeignKey(e => e.IdProducto)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<CierreCaja>(entity =>
            {
                entity.ToTable("cierres_caja");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
                entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
                entity.Property(e => e.FechaCierre).HasColumnName("fecha_cierre");
                entity.Property(e => e.TotalEfectivo)
                    .HasColumnName("total_efectivo")
                    .HasPrecision(12, 2);
                entity.Property(e => e.TotalTarjeta)
                    .HasColumnName("total_tarjeta")
                    .HasPrecision(12, 2);
                entity.Property(e => e.CantidadVentas).HasColumnName("cantidad_ventas");

                // Relación Cierre -> Usuario (RESTRICT por defecto en MySQL)
                entity.HasOne(e => e.Usuario)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuario)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}