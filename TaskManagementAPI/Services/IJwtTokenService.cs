using TaskManagementApi.Models;

namespace TaskManagementAPI.Services
{
    public interface IJwtTokenService
    {
       public string GenerateToken(User user);
    }
}
