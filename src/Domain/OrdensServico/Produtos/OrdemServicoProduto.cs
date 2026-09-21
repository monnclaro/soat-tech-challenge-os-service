using System.ComponentModel.DataAnnotations.Schema;
using Domain.Common;

namespace Domain.OrdensServico.Produtos;

public class OrdemServicoProduto : Entity
{
    public Guid Id { get; private set; }
    public Guid IdOrdemServico { get; private set; }
    public Guid IdProduto { get; private set; }
    public string NomeProduto { get; private set; }
    public decimal ValorUnitario { get; private set; }
    public decimal Quantidade { get; private set; }

    [NotMapped] public decimal Subtotal => ValorUnitario * Quantidade;

    public OrdemServicoProduto(Guid idOrdemServico, Guid idProduto, string nomeProduto, decimal valorUnitario, decimal quantidade)
    {
        Id = Guid.NewGuid();
        IdOrdemServico = idOrdemServico;
        IdProduto = idProduto;
        NomeProduto = nomeProduto;
        ValorUnitario = valorUnitario;
        Quantidade = quantidade;
    }
}
