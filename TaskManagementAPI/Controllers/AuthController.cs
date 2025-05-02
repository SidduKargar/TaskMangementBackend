using Microsoft.AspNetCore.Mvc;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Services;

namespace TaskManagementApi.Controllers
{
 
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IJwtTokenService _jwtTokenService;

        public AuthController(
            IUserService userService,
            IJwtTokenService jwtTokenService)
        {
            _userService = userService;
            _jwtTokenService = jwtTokenService;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token if successful.
        /// </summary>
        /// <param name="loginDto">User login credentials</param>
        /// <returns>User info with JWT token or Unauthorized</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Authenticate user 
            var user = await _userService.AuthenticateAsync(loginDto.Username, loginDto.Password);

            // Handle invalid login 
            if (user == null)
            {
                return Unauthorized(new { message = "Invalid username or password" });
            }

            // Generate JWT token 
            var token = _jwtTokenService.GenerateToken(user);

            // Return response 
            return Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Token = token
            });
        }

        /// <summary>
        /// Registers a new user and returns a JWT token.
        /// </summary>
        /// <param name="registerDto">User registration details</param>
        /// <returns>Created user info with JWT token or error</returns>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // Register new user 
            var user = await _userService.RegisterAsync(registerDto);

            // Handle duplicate username 
            if (user == null)
            {
                return BadRequest(new { message = "Username already exists" });
            }

            // Generate JWT token 
            var token = _jwtTokenService.GenerateToken(user);

            // Return response 
            return Ok(new
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                Token = token
            });
        }
    }
}
