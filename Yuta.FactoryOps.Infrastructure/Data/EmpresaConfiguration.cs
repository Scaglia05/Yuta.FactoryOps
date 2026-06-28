using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Yuta.FactoryOps.Domain.Entities;

namespace Yuta.FactoryOps.Infrastructure.Data
{
    public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
    {
        public void Configure(EntityTypeBuilder<Empresa> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.Cnpj).IsUnique();
        }
    }
}