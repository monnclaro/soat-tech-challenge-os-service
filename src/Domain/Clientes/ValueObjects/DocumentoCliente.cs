using Domain.Clientes.Enums;
using Domain.Common.ValueObjects;
using DomainException = Domain.Common.Exceptions.DomainException;

namespace Domain.Clientes.ValueObjects;

public sealed record DocumentoCliente
{
    public string Numero { get; }
    public TipoDocumentoCliente Tipo { get; }

    private DocumentoCliente(string numero, TipoDocumentoCliente tipo)
    {
        Numero = numero;
        Tipo = tipo;
    }

    public static DocumentoCliente Criar(string documentoBruto)
    {
        if (string.IsNullOrWhiteSpace(documentoBruto))
            throw new DomainException("O documento é obrigatório.");

        var digitos = new string(documentoBruto.Where(char.IsDigit).ToArray());

        if (string.IsNullOrWhiteSpace(digitos))
            throw new DomainException("O documento informado é inválido.");

        if (digitos.Length != 11 && digitos.Length != 14)
            throw new DomainException("O documento deve conter 11 dígitos (CPF) ou 14 dígitos (CNPJ).");

        var tipo = digitos.Length == 11
            ? TipoDocumentoCliente.Cpf
            : TipoDocumentoCliente.Cnpj;

        if (!ValidarDigitos(digitos, tipo))
            throw new DomainException($"{tipo} inválido: '{digitos}'.");

        return new DocumentoCliente(digitos, tipo);
    }

    private static bool ValidarDigitos(string documento, TipoDocumentoCliente tipo)
    {
        return tipo switch
        {
            TipoDocumentoCliente.Cpf => ValidarCpf(documento),
            TipoDocumentoCliente.Cnpj => ValidarCnpj(documento),
            _ => false
        };
    }

    private static bool ValidarCpf(string cpf) => CpfChecksum.Valido(cpf);

    private static bool ValidarCnpj(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1)
            return false;

        int[] pesos1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
        int[] pesos2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

        int soma = 0;
        for (int i = 0; i < 12; i++)
            soma += (cnpj[i] - '0') * pesos1[i];

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;
        if (digito1 != cnpj[12] - '0') return false;

        soma = 0;
        for (int i = 0; i < 13; i++)
            soma += (cnpj[i] - '0') * pesos2[i];

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return digito2 == cnpj[13] - '0';
    }
}
