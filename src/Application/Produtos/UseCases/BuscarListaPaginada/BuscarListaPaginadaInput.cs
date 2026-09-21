using SharedKernel.DTOs;

namespace Application.Produtos.UseCases.BuscarListaPaginada;

public record BuscarListaPaginadaInput(PagedRequest Paginacao);
