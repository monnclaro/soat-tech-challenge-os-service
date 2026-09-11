using Application.Servicos.DTOs;
using SharedKernel.DTOs;

namespace Application.Servicos.UseCases.BuscarListaPaginada;

public interface IBuscarListaPaginadaOutputPort
{
    void Ok(PagedResult<ServicoOutput> resultado);
}
