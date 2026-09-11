using Domain.Clientes;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Clientes;

public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
{
    public void Configure(EntityTypeBuilder<Cliente> builder)
    {
        builder.ToTable("cliente");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Nome)
               .HasColumnName("nome")
               .HasMaxLength(150)
               .IsRequired();

        builder.Property(c => c.Documento)
               .HasColumnName("documento")
               .HasMaxLength(14)
               .IsRequired();

        builder.HasIndex(c => c.Documento).IsUnique();

        builder.Property(c => c.TipoDocumento)
               .HasColumnName("tipo_documento")
               .IsRequired();

        builder.Property(c => c.Ativo)
               .HasColumnName("ativo")
               .HasDefaultValue(true)
               .IsRequired();

        builder.Property(x => x.DataCriacao)
               .HasColumnName("data_criacao")
               .IsRequired();

        builder.HasMany(x => x.Veiculos)
               .WithOne()
               .HasForeignKey(v => v.IdCliente)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
