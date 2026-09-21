using System.Diagnostics.CodeAnalysis;
using Domain.Usuarios;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Usuarios;

[ExcludeFromCodeCoverage]
public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuario");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");

        builder.Property(x => x.Nome)
            .HasColumnName("nome")
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(x => x.SenhaHash)
            .HasColumnName("senha_hash")
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(x => x.Cpf)
            .HasColumnName("cpf")
            .IsRequired()
            .HasMaxLength(11);

        builder.HasIndex(x => x.Cpf).IsUnique();

        builder.Property(x => x.Ativo)
            .HasColumnName("ativo")
            .HasDefaultValue(true)
            .IsRequired();

        builder.HasMany(x => x.Roles)
            .WithOne()
            .HasForeignKey(x => x.IdUsuario);
    }
}
