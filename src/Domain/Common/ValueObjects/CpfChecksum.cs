namespace Domain.Common.ValueObjects;

// Algoritmo de validação de dígitos verificadores de CPF, reaproveitado tanto
// por Cliente (DocumentoCliente) quanto por Usuario (login do back-office).
public static class CpfChecksum
{
    public static bool Valido(string cpf)
    {
        if (cpf.Length != 11 || cpf.Distinct().Count() == 1)
            return false;

        int soma = 0;
        for (int i = 0; i < 9; i++)
            soma += (cpf[i] - '0') * (10 - i);

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;
        if (digito1 != cpf[9] - '0') return false;

        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += (cpf[i] - '0') * (11 - i);

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return digito2 == cpf[10] - '0';
    }
}
