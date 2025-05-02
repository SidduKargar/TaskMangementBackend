using TaskManagementAPI.DTOs;
using TaskManagementApi.Models;

namespace TaskManagementAPI.Services
{
    public interface IUserService
    {
        Task<User> AuthenticateAsync(string username, string password);
        Task<User> RegisterAsync(RegisterDto registerDto);
    }
}
