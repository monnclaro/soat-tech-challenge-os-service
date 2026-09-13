namespace Application.OrdensServico.UseCases.BuscarHistorico;

public interface IBuscarHistoricoOutputPort
{
    void NaoEncontrado();
    void Ok(IReadOnlyList<HistoricoStatusOutput> historico);
}
