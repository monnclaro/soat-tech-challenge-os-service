using System.Diagnostics.CodeAnalysis;
using Domain.Clientes.Veiculos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Clientes;

[ExcludeFromCodeCoverage]
public class VeiculoConfiguration : IEntityTypeConfiguration<Veiculo>
{
    public void Configure(EntityTypeBuilder<Veiculo> builder)
    {
        builder.ToTable("veiculo");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.IdCliente).HasColumnName("id_cliente");

        builder.Property(v => v.Placa)
            .HasColumnName("placa")
            .HasMaxLength(8)
            .IsRequired();

        builder.HasIndex(v => v.Placa).IsUnique();

        builder.Property(x => x.Marca)
            .HasColumnName("marca")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Modelo)
            .HasColumnName("modelo")
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(x => x.Ano)
            .HasColumnName("ano")
            .IsRequired();
    }
}
