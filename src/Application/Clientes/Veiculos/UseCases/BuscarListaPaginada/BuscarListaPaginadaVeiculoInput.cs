using SharedKernel.DTOs;

namespace Application.Clientes.Veiculos.UseCases.BuscarListaPaginada;

public record BuscarListaPaginadaVeiculoInput(Guid IdCliente, PagedRequest Paginacao);
