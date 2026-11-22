using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
    /// <summary>
    /// Authentication Controller for user registration and issuing JSON Web Tokens (JWT).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Registers a new user account.
        /// Role can be "User" (default) or "Admin".
        /// </summary>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request)
        {
            var result = await _authService.RegisterAsync(request);
            return StatusCode(StatusCodes.Status201Created, result);
        }

        /// <summary>
        /// Authenticates user and returns a signed JWT token with claims.
        /// Demo accounts:
        ///   Admin: username "admin", password "Admin@123"
        ///   User:  username "user",  password "User@123"
        /// Or any user registered via POST /api/auth/register.
        /// </summary>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.AuthenticateAsync(request);

            if (result == null)
            {
                return Unauthorized(new { message = "Invalid username or password." });
            }

            return Ok(result);
        }
    }
}
