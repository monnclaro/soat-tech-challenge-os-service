using Domain.OrdensServico.Historico;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations.OrdensServico;

public class HistoricoStatusOrdemServicoConfiguration : IEntityTypeConfiguration<HistoricoStatusOrdemServico>
{
    public void Configure(EntityTypeBuilder<HistoricoStatusOrdemServico> builder)
    {
        builder.ToTable("historico_status_ordem_servico");

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("id");
        builder.Property(x => x.IdOrdemServico).HasColumnName("id_ordem_servico").IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").IsRequired();
        builder.Property(x => x.OcorridoEm).HasColumnName("ocorrido_em").IsRequired();

        builder.HasIndex(x => x.IdOrdemServico);
    }
}
