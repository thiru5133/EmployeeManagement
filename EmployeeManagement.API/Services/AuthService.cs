using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace EmployeeManagement.API.Services
{
    /// <summary>
    /// Authentication Service responsible for user registration,
    /// password hashing with salt, credentials verification, and JWT generation.
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly AppDbContext? _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        // In-memory user cache for test isolation when DB context is omitted
        private static readonly List<User> InMemoryUsers = new();

        // Fallback demo users for quick testing
        private static readonly Dictionary<string, (string Password, string Role)> FallbackUsers = new(StringComparer.OrdinalIgnoreCase)
        {
            { "admin", ("Admin@123", "Admin") },
            { "user",  ("User@123",  "User") }
        };

        public AuthService(IConfiguration configuration, ILogger<AuthService> logger, AppDbContext? context = null)
        {
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }

        // ── REGISTER ─────────────────────────────────────────────────────────────
        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request)
        {
            _logger.LogInformation("Attempting registration for username: {Username}", request.Username);

            // Check if username or email is already taken
            bool exists;
            if (_context != null)
            {
                exists = await _context.Users
                    .AsNoTracking()
                    .AnyAsync(u => u.Username.ToLower() == request.Username.ToLower() ||
                                   u.Email.ToLower() == request.Email.ToLower());
            }
            else
            {
                lock (InMemoryUsers)
                {
                    exists = InMemoryUsers.Any(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase) ||
                                                    u.Email.Equals(request.Email, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (exists)
            {
                throw new ArgumentException($"User '{request.Username}' or email '{request.Email}' is already registered.");
            }

            // Hash password with a unique cryptographic salt
            CreatePasswordHash(request.Password, out var passwordHash, out var passwordSalt);

            var newUser = new User
            {
                Username     = request.Username.Trim(),
                Email        = request.Email.Trim(),
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                Role         = string.IsNullOrWhiteSpace(request.Role) ? "User" : request.Role,
                CreatedAt    = DateTime.UtcNow
            };

            if (_context != null)
            {
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
            }
            else
            {
                lock (InMemoryUsers)
                {
                    newUser.Id = InMemoryUsers.Count + 1;
                    InMemoryUsers.Add(newUser);
                }
            }

            _logger.LogInformation("User {Username} registered successfully with role {Role}.", newUser.Username, newUser.Role);

            return new RegisterResponseDto
            {
                Id       = newUser.Id,
                Username = newUser.Username,
                Email    = newUser.Email,
                Role     = newUser.Role,
                Message  = "User registered successfully."
            };
        }

        // ── LOGIN ────────────────────────────────────────────────────────────────
        public async Task<LoginResponseDto?> AuthenticateAsync(LoginRequestDto request)
        {
            _logger.LogInformation("Authentication attempt for user: {Username}", request.Username);

            string role;

            // 1. Check registered database/in-memory users
            User? matchedUser = null;
            if (_context != null)
            {
                matchedUser = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == request.Username.ToLower());
            }
            else
            {
                lock (InMemoryUsers)
                {
                    matchedUser = InMemoryUsers.FirstOrDefault(u => u.Username.Equals(request.Username, StringComparison.OrdinalIgnoreCase));
                }
            }

            if (matchedUser != null)
            {
                if (!VerifyPasswordHash(request.Password, matchedUser.PasswordHash, matchedUser.PasswordSalt))
                {
                    _logger.LogWarning("Authentication failed (bad password) for user: {Username}", request.Username);
                    return null;
                }
                role = matchedUser.Role;
            }
            // 2. Fallback to demo users
            else if (FallbackUsers.TryGetValue(request.Username, out var fallback) && fallback.Password == request.Password)
            {
                role = fallback.Role;
            }
            else
            {
                _logger.LogWarning("User not found or credentials invalid: {Username}", request.Username);
                return null;
            }

            // Generate JWT
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
                    new Claim(ClaimTypes.Role, role),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = expiration,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyBytes), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("Token issued successfully for {Username} with role {Role}", request.Username, role);

            return new LoginResponseDto
            {
                Token      = tokenString,
                Expiration = expiration,
                Username   = request.Username,
                Role       = role
            };
        }

        // ── PASSWORD HASHING HELPERS ─────────────────────────────────────────────
        private static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using var hmac = new HMACSHA512();
            passwordSalt = Convert.ToBase64String(hmac.Key);
            passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
        }

        private static bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            var key = Convert.FromBase64String(storedSalt);
            using var hmac = new HMACSHA512(key);
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return computedHash == storedHash;
        }
    }
}
