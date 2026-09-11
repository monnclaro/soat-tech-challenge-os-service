using Domain.OrdensServico.Servicos;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.OrdensServico;

public class OrdemServicoServicoConfiguration : IEntityTypeConfiguration<OrdemServicoServico>
{
    public void Configure(EntityTypeBuilder<OrdemServicoServico> builder)
    {
        builder.ToTable("ordem_servico_servico");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(x => x.IdOrdemServico).HasColumnName("idordemservico").IsRequired();
        builder.Property(x => x.IdServico).HasColumnName("idservico").IsRequired();

        builder.Property(x => x.NomeServico)
            .HasColumnName("nomeservico")
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Valor)
            .HasColumnName("valorunitario")
            .HasColumnType("decimal(10,2)")
            .IsRequired();
    }
}
