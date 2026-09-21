namespace Application.Login.UseCases.Interfaces;

public interface IPasswordHasher
{
    bool Verificar(string senha, string hash);
}
