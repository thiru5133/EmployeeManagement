using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EmployeeManagement.API.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagement.API.Services
{
    /// <summary>
    /// Day 10 — Authentication Service responsible for verifying credentials
    /// and generating signed JWT tokens with Claims and Roles.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        // Sample users for demo & testing Gate 2 checkpoint
        private static readonly Dictionary<string, (string Password, string Role)> Users = new(StringComparer.OrdinalIgnoreCase)
        {
            { "admin", ("Admin@123", "Admin") },
            { "user",  ("User@123",  "User") }
        };

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request)
        {
            _logger.LogInformation("Authentication attempt for user: {Username}", request.Username);

            if (!Users.TryGetValue(request.Username, out var userRecord) || userRecord.Password != request.Password)
            {
                _logger.LogWarning("Authentication failed for user: {Username}", request.Username);
                return Task.FromResult<LoginResponseDto?>(null);
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtKey = _configuration["Jwt:Key"] ?? "Week2TrainingSuperSecretKeyForJwtTokenGeneration2026!";
            var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
            var issuer = _configuration["Jwt:Issuer"] ?? "EmployeeManagementAPI";
            var audience = _configuration["Jwt:Audience"] ?? "EmployeeManagementClient";
            var durationMinutes = double.TryParse(_configuration["Jwt:DurationInMinutes"], out var d) ? d : 60;
            var expiration = DateTime.UtcNow.AddMinutes(durationMinutes);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, request.Username),
                    new Claim(ClaimTypes.Name, request.Username),
                    new Claim(ClaimTypes.Role, userRecord.Role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = expiration,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("Token issued successfully for {Username} with role {Role}", request.Username, userRecord.Role);

            return Task.FromResult<LoginResponseDto?>(new LoginResponseDto
            {
                Token = tokenString,
                Expiration = expiration,
                Username = request.Username,
                Role = userRecord.Role
            });
        }
    }
}
