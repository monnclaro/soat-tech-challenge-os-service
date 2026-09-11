namespace Application.OrdensServico.UseCases.BuscarOrdemServico;

public interface IBuscarOrdemServicoOutputPort
{
    void NaoEncontrado();
    void Ok(OrdemServicoOutput output);
}
