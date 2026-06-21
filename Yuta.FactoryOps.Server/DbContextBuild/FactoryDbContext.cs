using Microsoft.EntityFrameworkCore;
using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Server.DbContextBuild
{
    public class FactoryDbContext : DbContext 
    {
        public FactoryDbContext(DbContextOptions<FactoryDbContext> options) : base(options)
        {
        }

        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Empresa>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => e.Cnpj).IsUnique();
            });

            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();

                entity.HasOne<Empresa>()
                      .WithMany()
                      .HasForeignKey(u => u.EmpresaId)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}