using System.Text.RegularExpressions;

using DomainException = Domain.Common.Exceptions.DomainException;

namespace Domain.Clientes.Veiculos.ValueObjects;

public sealed record Placa
{
    public string Valor { get; }

    private static readonly Regex Antiga = new(@"^[A-Z]{3}-?\d{4}$", RegexOptions.Compiled);
    private static readonly Regex Mercosul = new(@"^[A-Z]{3}\d[A-Z]\d{2}$", RegexOptions.Compiled);

    private Placa(string valor) => Valor = valor;

    public static Placa Criar(string placa)
    {
        if (string.IsNullOrWhiteSpace(placa))
            throw new DomainException("A placa é obrigatória.");

        var normalizada = placa.Trim().ToUpper().Replace("-", "");

        if (!Antiga.IsMatch(normalizada) && !Mercosul.IsMatch(normalizada))
            throw new DomainException("Placa inválida. Use o formato ABC1234 (antiga) ou ABC1D23 (Mercosul).");

        return new Placa(normalizada);
    }
}
