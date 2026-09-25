using Microsoft.EntityFrameworkCore;
using Kiosco.Api.Models;

namespace Kiosco.Api
{
    public class KioscoContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
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

            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("productos");
                entity.Property(e => e.Id).HasColumnName("id");
                entity.Property(e => e.Nombre).HasColumnName("nombre");
                entity.Property(e => e.PrecioCosto)
                    .HasColumnName("precio_costo")
                    .HasPrecision(12, 2);
                entity.Property(e => e.PrecioVenta)
                    .HasColumnName("precio_venta")
                    .HasPrecision(12, 2);
            });
        }
    }
}