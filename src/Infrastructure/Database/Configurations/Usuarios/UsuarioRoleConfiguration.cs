using System.Diagnostics.CodeAnalysis;
using Domain.Usuarios.Roles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Usuarios;

[ExcludeFromCodeCoverage]
public class UsuarioRoleConfiguration : IEntityTypeConfiguration<UsuarioRole>
{
    public void Configure(EntityTypeBuilder<UsuarioRole> builder)
    {
        builder.ToTable("usuario_role");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.IdUsuario).HasColumnName("id_usuario");

        builder.Property(x => x.Role)
            .HasColumnName("role")
            .IsRequired()
            .HasMaxLength(150);
    }
}
