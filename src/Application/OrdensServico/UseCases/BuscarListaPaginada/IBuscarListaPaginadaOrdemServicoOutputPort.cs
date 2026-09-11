using SharedKernel.DTOs;

namespace Application.OrdensServico.UseCases.BuscarListaPaginada;

public interface IBuscarListaPaginadaOrdemServicoOutputPort
{
    void Ok(PagedResult<OrdemServicoOutput> resultado);
}
