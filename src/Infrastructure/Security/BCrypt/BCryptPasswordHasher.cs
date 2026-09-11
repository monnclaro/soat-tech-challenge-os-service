using Application.Login.UseCases.Interfaces;
using SharedKernel.Interfaces;

namespace Infrastructure.Security.BCrypt;

public class BCryptPasswordHasher : IPasswordHasher, IScoped
{
    public bool Verificar(string senha, string hash)
    {
        return global::BCrypt.Net.BCrypt.Verify(senha, hash);
    }
}
