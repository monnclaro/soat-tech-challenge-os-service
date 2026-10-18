using System.Diagnostics.CodeAnalysis;
using Domain.OrdensServico;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.OrdensServico;

[ExcludeFromCodeCoverage]
public class OrdemServicoConfiguration : IEntityTypeConfiguration<OrdemServico>
{
    public void Configure(EntityTypeBuilder<OrdemServico> builder)
    {
        builder.ToTable("ordem_servico");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.IdCliente).HasColumnName("id_cliente").IsRequired();
        builder.Property(x => x.IdVeiculo).HasColumnName("id_veiculo").IsRequired();
        builder.Property(x => x.DataCriacao).HasColumnName("data_criacao").IsRequired();
        builder.Property(x => x.DataInicioExecucao).HasColumnName("data_inicio_execucao");
        builder.Property(x => x.DataFinalizacao).HasColumnName("data_finalizacao");
        builder.Property(x => x.Status).HasColumnName("status").IsRequired();
        builder.Property(x => x.ValorTotal).HasColumnName("valor_total").HasColumnType("decimal(10,2)").IsRequired();

        builder.HasMany(x => x.Servicos)
            .WithOne()
            .HasForeignKey(x => x.IdOrdemServico)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Produtos)
            .WithOne()
            .HasForeignKey(x => x.IdOrdemServico)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
