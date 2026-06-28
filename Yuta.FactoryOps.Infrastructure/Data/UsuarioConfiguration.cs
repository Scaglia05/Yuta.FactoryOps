using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Infrastructure.Data
{
    public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.HasKey(u => u.Id);
            builder.HasIndex(u => u.Email).IsUnique();
            builder.HasOne<Empresa>()
                  .WithMany()
                  .HasForeignKey(u => u.EmpresaId)
                  .OnDelete(DeleteBehavior.Restrict);
        }
    }
}