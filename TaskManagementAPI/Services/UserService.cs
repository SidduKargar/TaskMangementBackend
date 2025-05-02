using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Models;
using TaskManagementAPI.DataAccess;
using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Services
{

    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _context;

        public UserService(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Authenticates a user by username and password.
        /// </summary>
        /// <param name="username">The username of the user.</param>
        /// <param name="password">The password of the user.</param>
        /// <returns>The authenticated user or null if authentication fails.</returns>
        public async Task<User> AuthenticateAsync(string username, string password)
        {
            // Find user by username 
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == username);

            // Validate password 
            if (user == null || password != user.Password)
            {
                return null;
            }

            // Return authenticated user
            return user;
        }

        /// <summary>
        /// Registers a new user with the given registration details.
        /// </summary>
        /// <param name="registerDto">The registration details.</param>
        /// <returns>The created user or null if username already exists.</returns>
        public async Task<User> RegisterAsync(RegisterDto registerDto)
        {
            // Check if username already exists 
            if (await _context.Users.AnyAsync(u => u.Username == registerDto.Username))
            {
                return null;
            }

            // Create new user entity 
            var user = new User
            {
                Username = registerDto.Username,
                Email = registerDto.Email,
                Password = registerDto.Password,
                Role = "User" // Default role
            };

            // Save new user to database
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return user;
        }
    }
}
