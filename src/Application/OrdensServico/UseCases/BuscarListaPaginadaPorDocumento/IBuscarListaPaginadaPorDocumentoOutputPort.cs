using SharedKernel.DTOs;

namespace Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;

public interface IBuscarListaPaginadaPorDocumentoOutputPort
{
    void Ok(PagedResult<OrdemServicoPorDocumentoOutput> resultado);
}
