using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace Yuta.FactoryOps.Models
{
    public class Empresa
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string RazaoSocial { get; set; } = string.Empty;
        public string Cnpj { get; set; } = string.Empty;
        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;
        public bool Ativa { get; set; } = true;
    }

    // A lógica de banco da Empresa fica isolada aqui
    public class EmpresaConfiguration : IEntityTypeConfiguration<Empresa>
    {
        public void Configure(EntityTypeBuilder<Empresa> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasIndex(e => e.Cnpj).IsUnique();

            builder.Property(e => e.RazaoSocial).HasMaxLength(150).IsRequired();
            builder.Property(e => e.Cnpj).HasMaxLength(14).IsRequired();
        }
    }
}