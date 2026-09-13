using Api.Extensions.Markers;
using Application.OrdensServico.UseCases;
using Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Api.Presenters.OrdensServico;

public class BuscarListaPaginadaPorDocumentoPresenter : IBuscarListaPaginadaPorDocumentoOutputPort, IPresenter
{
    public IActionResult? Result { get; private set; }
    public void Ok(PagedResult<OrdemServicoPorDocumentoOutput> resultado) => Result = new OkObjectResult(resultado);
}
