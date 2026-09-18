using Microsoft.EntityFrameworkCore;
using Kiosco.Api.Models;

namespace Kiosco.Api
{
    public class KioscoContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
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
        }
    }
}