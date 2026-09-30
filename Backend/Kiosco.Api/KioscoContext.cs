using Microsoft.EntityFrameworkCore;
using Kiosco.Api.Models;

namespace Kiosco.Api
{
    public class KioscoContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
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
        }
    }
}