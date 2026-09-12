using System.Diagnostics.CodeAnalysis;
using Domain.OrdensServico.Produtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.OrdensServico;

[ExcludeFromCodeCoverage]
public class OrdemServicoProdutoConfiguration : IEntityTypeConfiguration<OrdemServicoProduto>
{
    public void Configure(EntityTypeBuilder<OrdemServicoProduto> builder)
    {
        builder.ToTable("ordem_servico_produto");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.IdOrdemServico).HasColumnName("idordemservico").IsRequired();
        builder.Property(x => x.IdProduto).HasColumnName("idproduto").IsRequired();

        builder.Property(x => x.NomeProduto).HasColumnName("nomeproduto")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.ValorUnitario)
            .HasColumnName("valorunitario")
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(x => x.Quantidade)
            .HasColumnName("quantidade")
            .HasColumnType("decimal(10,2)")
            .IsRequired();
    }
}
