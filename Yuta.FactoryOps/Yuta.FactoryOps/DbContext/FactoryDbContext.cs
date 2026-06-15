using Microsoft.EntityFrameworkCore;
using Yuta.FactoryOps.Models; 

namespace Yuta.FactoryOps.Data
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

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(FactoryDbContext).Assembly);
        }
    }
}