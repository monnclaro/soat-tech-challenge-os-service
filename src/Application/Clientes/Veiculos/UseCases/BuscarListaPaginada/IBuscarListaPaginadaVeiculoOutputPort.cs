using SharedKernel.DTOs;

namespace Application.Clientes.Veiculos.UseCases.BuscarListaPaginada;

public interface IBuscarListaPaginadaVeiculoOutputPort
{
    void Ok(PagedResult<VeiculoOutput> resultado);
}
