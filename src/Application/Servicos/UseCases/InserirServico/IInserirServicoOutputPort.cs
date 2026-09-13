using Application.Servicos.DTOs;

namespace Application.Servicos.UseCases.InserirServico;

public interface IInserirServicoOutputPort
{
    void Ok(ServicoOutput output);
}
