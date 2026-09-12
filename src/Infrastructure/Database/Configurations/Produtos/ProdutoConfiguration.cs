using System.Diagnostics.CodeAnalysis;
using Domain.Produtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.Produtos;

[ExcludeFromCodeCoverage]
public class ProdutoConfiguration : IEntityTypeConfiguration<Produto>
{
    public void Configure(EntityTypeBuilder<Produto> builder)
    {
        builder.ToTable("produto");

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

        builder.Property(x => x.QuantidadeEmEstoque)
               .HasColumnName("quantidade_estoque")
               .IsRequired();
    }
}
