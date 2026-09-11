using SharedKernel.DTOs;

namespace Application.OrdensServico.UseCases.BuscarListaPaginadaPorDocumento;

// CallerDocumento: preenchido só quando quem chama logou como Cliente (via
// CPF) — null para Admin. Usado pra impedir um Cliente consultar as OS de
// outro documento; Admin nunca é restringido por isso.
public record BuscarListaPaginadaPorDocumentoInput(string Documento, PagedRequest Paginacao, string? CallerDocumento = null);
