using Api.Presenters.Authentication;
using Api.Presenters.Clientes;
using Api.Presenters.Clientes.Veiculos;
using Api.Presenters.OrdensServico;
using Api.Presenters.Produtos;
using Api.Presenters.Servicos;
using Application.Clientes.UseCases;
using Application.Clientes.Veiculos.UseCases;
using Application.Login.UseCases.DTOs;
using Application.OrdensServico.UseCases;
using Application.Produtos.DTOs;
using Application.Servicos.DTOs;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using SharedKernel.DTOs;

namespace Tests.Api.Unit;

// As Presenters do Api layer traduzem o output port de cada use case num
// IActionResult — sem branching de negócio, só mapeamento 1:1. Cobertas aqui
// chamando cada método e conferindo o tipo (e, quando relevante, o conteúdo)
// do IActionResult resultante.
public class PresentersTests
{
    private static readonly ClienteOutput ClienteOutput = new(Guid.NewGuid(), "João Silva", "12345678909", true, DateTime.UtcNow);
    private static readonly VeiculoOutput VeiculoOutput = new(Guid.NewGuid(), Guid.NewGuid(), "ABC1234", "Toyota", "Corolla", 2020, DateTime.UtcNow);
    private static readonly ProdutoOutput ProdutoOutput = new(Guid.NewGuid(), "Óleo", "Óleo sintético", 79.90m, 10m);
    private static readonly ServicoOutput ServicoOutput = new(Guid.NewGuid(), "Troca de óleo", "Substituição do óleo", 120m);

    [Fact]
    public void LoginPresenter_MapeiaTodosOsCasos()
    {
        var presenter = new LoginPresenter();

        presenter.UsuarioNaoEncontrado();
        presenter.Result.Should().BeOfType<NotFoundResult>();

        presenter.SenhaInvalida();
        presenter.Result.Should().BeOfType<UnauthorizedResult>();

        presenter.LoginRealizado(new LoginOutput("token-jwt"));
        presenter.Result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public void ClientePresenters_MapeiamTodosOsCasos()
    {
        var atualizar = new AtualizarClientePresenter();
        atualizar.NaoEncontrado();
        atualizar.Result.Should().BeOfType<NotFoundResult>();
        atualizar.Ok(ClienteOutput);
        atualizar.Result.Should().BeOfType<OkObjectResult>();

        var buscar = new BuscarClientePresenter();
        buscar.NaoEncontrado();
        buscar.Result.Should().BeOfType<NotFoundResult>();
        buscar.Ok(ClienteOutput);
        buscar.Result.Should().BeOfType<OkObjectResult>();

        var listar = new BuscarListaPaginadaClientePresenter();
        listar.Ok(new PagedResult<ClienteOutput>([ClienteOutput], 1, 1, 10));
        listar.Result.Should().BeOfType<OkObjectResult>();

        var inserir = new InserirClientePresenter();
        inserir.DocumentoDuplicado("Documento já cadastrado.");
        inserir.Result.Should().BeOfType<ConflictObjectResult>();
        inserir.Ok(ClienteOutput);
        inserir.Result.Should().BeOfType<CreatedAtActionResult>();

        var remover = new RemoverClientePresenter();
        remover.NaoEncontrado();
        remover.Result.Should().BeOfType<NotFoundResult>();
        remover.Ok();
        remover.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void VeiculoPresenters_MapeiamTodosOsCasos()
    {
        var atualizar = new AtualizarVeiculoPresenter();
        atualizar.NaoEncontrado();
        atualizar.Result.Should().BeOfType<NotFoundResult>();
        atualizar.PlacaDuplicada("Placa já cadastrada.");
        atualizar.Result.Should().BeOfType<ConflictObjectResult>();
        atualizar.Ok(VeiculoOutput);
        atualizar.Result.Should().BeOfType<OkObjectResult>();

        var listar = new BuscarListaPaginadaVeiculoPresenter();
        listar.Ok(new PagedResult<VeiculoOutput>([VeiculoOutput], 1, 1, 10));
        listar.Result.Should().BeOfType<OkObjectResult>();

        var buscar = new BuscarVeiculoPresenter();
        buscar.NaoEncontrado();
        buscar.Result.Should().BeOfType<NotFoundResult>();
        buscar.Ok(VeiculoOutput);
        buscar.Result.Should().BeOfType<OkObjectResult>();

        var inserir = new InserirVeiculoPresenter();
        inserir.ClienteNaoEncontrado();
        inserir.Result.Should().BeOfType<NotFoundObjectResult>();
        inserir.PlacaDuplicada("Placa já cadastrada.");
        inserir.Result.Should().BeOfType<ConflictObjectResult>();
        inserir.Ok(VeiculoOutput);
        inserir.Result.Should().BeOfType<CreatedAtActionResult>();

        var remover = new RemoverVeiculoPresenter();
        remover.NaoEncontrado();
        remover.Result.Should().BeOfType<NotFoundResult>();
        remover.Ok();
        remover.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ProdutoPresenters_MapeiamTodosOsCasos()
    {
        var atualizar = new AtualizarProdutoPresenter();
        atualizar.NaoEncontrado();
        atualizar.Result.Should().BeOfType<NotFoundResult>();
        atualizar.Ok(ProdutoOutput);
        atualizar.Result.Should().BeOfType<OkObjectResult>();

        var listar = new BuscarListaPaginadaProdutoPresenter();
        listar.Ok(new PagedResult<ProdutoOutput>([ProdutoOutput], 1, 1, 10));
        listar.Result.Should().BeOfType<OkObjectResult>();

        var buscar = new BuscarProdutoPresenter();
        buscar.NaoEncontrado();
        buscar.Result.Should().BeOfType<NotFoundResult>();
        buscar.Ok(ProdutoOutput);
        buscar.Result.Should().BeOfType<OkObjectResult>();

        var incrementar = new IncrementarEstoquePresenter();
        incrementar.NaoEncontrado();
        incrementar.Result.Should().BeOfType<NotFoundResult>();
        incrementar.Ok(ProdutoOutput);
        incrementar.Result.Should().BeOfType<OkObjectResult>();

        var inserir = new InserirProdutoPresenter();
        inserir.Ok(ProdutoOutput);
        inserir.Result.Should().BeOfType<CreatedAtActionResult>();

        var remover = new RemoverProdutoPresenter();
        remover.Ok();
        remover.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void ServicoPresenters_MapeiamTodosOsCasos()
    {
        var atualizar = new AtualizarServicoPresenter();
        atualizar.NaoEncontrado();
        atualizar.Result.Should().BeOfType<NotFoundResult>();
        atualizar.Ok(ServicoOutput);
        atualizar.Result.Should().BeOfType<OkObjectResult>();

        var listar = new BuscarListaPaginadaPresenter();
        listar.Ok(new PagedResult<ServicoOutput>([ServicoOutput], 1, 1, 10));
        listar.Result.Should().BeOfType<OkObjectResult>();

        var buscar = new BuscarServicoPresenter();
        buscar.NaoEncontrado();
        buscar.Result.Should().BeOfType<NotFoundResult>();
        buscar.Ok(ServicoOutput);
        buscar.Result.Should().BeOfType<OkObjectResult>();

        var inserir = new InserirServicoPresenter();
        inserir.Ok(ServicoOutput);
        inserir.Result.Should().BeOfType<CreatedAtActionResult>();

        var remover = new RemoverServicoPresenter();
        remover.Ok();
        remover.Result.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public void OrdemServicoPresenters_MapeiamTodosOsCasos()
    {
        var clienteOutput = new OrdemServicoClienteOutput(Guid.NewGuid(), "João Silva", "12345678909");
        var veiculoOutput = new OrdemServicoVeiculoOutput(Guid.NewGuid(), "ABC1234", "Toyota", "Corolla", 2020);
        var osOutput = new OrdemServicoOutput(
            Guid.NewGuid(), clienteOutput, veiculoOutput, DateTime.UtcNow, null, null, "Recebida", 0m, [], []);
        var osPorDocumentoOutput = new OrdemServicoPorDocumentoOutput(
            Guid.NewGuid(), "Recebida",
            new OrdemServicoClientePorDocumentoOutput("João Silva", "12345678909"),
            new OrdemServicoVeiculoPorDocumentoOutput("ABC1234", "Toyota", "Corolla", 2020),
            []);

        var buscar = new BuscarOrdemServicoPresenter();
        buscar.NaoEncontrado();
        buscar.Result.Should().BeOfType<NotFoundResult>();
        buscar.Ok(osOutput);
        buscar.Result.Should().BeOfType<OkObjectResult>();

        var listar = new BuscarListaPaginadaOrdemServicoPresenter();
        listar.Ok(new PagedResult<OrdemServicoOutput>([osOutput], 1, 1, 10));
        listar.Result.Should().BeOfType<OkObjectResult>();

        var listarPorDocumento = new BuscarListaPaginadaPorDocumentoPresenter();
        listarPorDocumento.Ok(new PagedResult<OrdemServicoPorDocumentoOutput>([osPorDocumentoOutput], 1, 1, 10));
        listarPorDocumento.Result.Should().BeOfType<OkObjectResult>();

        var historico = new BuscarHistoricoPresenter();
        historico.NaoEncontrado();
        historico.Result.Should().BeOfType<NotFoundResult>();
        historico.Ok([new HistoricoStatusOutput("Recebida", DateTime.UtcNow)]);
        historico.Result.Should().BeOfType<OkObjectResult>();

        var inserir = new InserirOrdemServicoPresenter();
        inserir.ClienteNaoEncontrado();
        inserir.Result.Should().BeOfType<NotFoundObjectResult>();
        inserir.VeiculoNaoPertenceAoCliente("João Silva");
        inserir.Result.Should().BeOfType<BadRequestObjectResult>();
        inserir.Ok(Guid.NewGuid());
        inserir.Result.Should().BeOfType<CreatedAtActionResult>();

        var iniciarDiagnostico = new IniciarDiagnosticoPresenter();
        iniciarDiagnostico.NaoEncontrado();
        iniciarDiagnostico.Result.Should().BeOfType<NotFoundResult>();
        iniciarDiagnostico.Ok();
        iniciarDiagnostico.Result.Should().BeOfType<OkResult>();

        var registrarDiagnostico = new RegistrarDiagnosticoPresenter();
        registrarDiagnostico.NaoEncontrado();
        registrarDiagnostico.Result.Should().BeOfType<NotFoundResult>();
        registrarDiagnostico.Ok();
        registrarDiagnostico.Result.Should().BeOfType<OkResult>();

        var aprovarPagamento = new AprovarPagamentoPresenter();
        aprovarPagamento.NaoEncontrado();
        aprovarPagamento.Result.Should().BeOfType<NotFoundResult>();
        aprovarPagamento.Ok();
        aprovarPagamento.Result.Should().BeOfType<OkResult>();

        var cancelar = new CancelarPresenter();
        cancelar.NaoEncontrado();
        cancelar.Result.Should().BeOfType<NotFoundResult>();
        cancelar.Ok();
        cancelar.Result.Should().BeOfType<OkResult>();

        var finalizar = new FinalizarPresenter();
        finalizar.NaoEncontrado();
        finalizar.Result.Should().BeOfType<NotFoundResult>();
        finalizar.Ok();
        finalizar.Result.Should().BeOfType<OkResult>();

        var entregar = new EntregarPresenter();
        entregar.NaoEncontrado();
        entregar.Result.Should().BeOfType<NotFoundResult>();
        entregar.Ok();
        entregar.Result.Should().BeOfType<OkResult>();

        var remover = new RemoverOrdemServicoPresenter();
        remover.NaoEncontrado();
        remover.Result.Should().BeOfType<NotFoundResult>();
        remover.Ok();
        remover.Result.Should().BeOfType<NoContentResult>();
    }
}
