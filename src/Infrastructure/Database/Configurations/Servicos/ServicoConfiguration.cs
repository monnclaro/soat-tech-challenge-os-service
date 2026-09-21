using System.Diagnostics.CodeAnalysis;
using Domain.Servicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Servicos;

[ExcludeFromCodeCoverage]
public class ServicoConfiguration : IEntityTypeConfiguration<Servico>
{
    public void Configure(EntityTypeBuilder<Servico> builder)
    {
        builder.ToTable("servico");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Nome)
            .HasColumnName("nome")
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.Descricao)
            .HasColumnName("descricao")
            .HasMaxLength(500);

        builder.Property(x => x.Valor)
            .HasColumnName("valor")
            .HasColumnType("decimal(10,2)")
            .IsRequired();
    }
}
