using Microsoft.EntityFrameworkCore;
using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Infrastructure.Data
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

            modelBuilder.ApplyConfiguration(new EmpresaConfiguration());
            modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
        }
    }
}