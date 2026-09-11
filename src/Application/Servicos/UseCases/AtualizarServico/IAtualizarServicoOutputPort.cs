using Application.Servicos.DTOs;

namespace Application.Servicos.UseCases.AtualizarServico;

public interface IAtualizarServicoOutputPort
{
    void NaoEncontrado();
    void Ok(ServicoOutput output);
}
