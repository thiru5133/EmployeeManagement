using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Services
{
    public interface IAuthService
    {
        /// <summary>
        /// Registers a new user with a hashed password.
        /// </summary>
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request);

        /// <summary>
        /// Authenticates the user credentials and generates a signed JWT token if valid.
        /// Returns null if credentials are invalid.
        /// </summary>
        Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request);
    }
}
