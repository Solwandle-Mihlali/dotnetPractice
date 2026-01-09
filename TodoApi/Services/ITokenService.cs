using TodoApi.Models;

namespace TodoApi.Services
{
    public interface ITokenService
    {
        string GenerateToken(UsersModel user);
    }
}
