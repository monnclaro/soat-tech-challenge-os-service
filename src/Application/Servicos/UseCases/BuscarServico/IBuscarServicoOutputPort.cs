using Application.Servicos.DTOs;

namespace Application.Servicos.UseCases.BuscarServico;

public interface IBuscarServicoOutputPort
{
    void NaoEncontrado();
    void Ok(ServicoOutput output);
}
