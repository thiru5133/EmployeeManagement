using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace EmployeeManagement.Tests
{
    /// <summary>
    /// Authentication and User Registration Unit Tests.
    /// </summary>
    public class AuthServiceTests
    {
        private readonly IConfiguration _configuration;

        public AuthServiceTests()
        {
            var configData = new Dictionary<string, string?>
            {
                { "Jwt:Key", "Week2TrainingSuperSecretKeyForJwtTokenGeneration2026!" },
                { "Jwt:Issuer", "EmployeeManagementAPI" },
                { "Jwt:Audience", "EmployeeManagementClient" },
                { "Jwt:DurationInMinutes", "60" }
            };

            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(configData)
                .Build();
        }

        private AuthService CreateService()
            => new(_configuration, NullLogger<AuthService>.Instance);

        // ── REGISTRATION TESTS ───────────────────────────────────────────────────

        [Fact]
        public async Task RegisterAsync_WithValidData_ReturnsRegisteredUser()
        {
            // Arrange
            var service = CreateService();
            var uniqueUser = $"newuser_{Guid.NewGuid():N}";
            var request = new RegisterRequestDto
            {
                Username = uniqueUser,
                Email    = $"{uniqueUser}@example.com",
                Password = "SecurePassword123!",
                Role     = "User"
            };

            // Act
            var result = await service.RegisterAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(uniqueUser, result.Username);
            Assert.Equal("User", result.Role);
            Assert.True(result.Id > 0);
        }

        [Fact]
        public async Task RegisterAsync_AndThenLogin_ReturnsValidTokenWithRole()
        {
            // Arrange
            var service = CreateService();
            var uniqueUser = $"reguser_{Guid.NewGuid():N}";
            var password = "StrongPassword@2026";

            await service.RegisterAsync(new RegisterRequestDto
            {
                Username = uniqueUser,
                Email    = $"{uniqueUser}@example.com",
                Password = password,
                Role     = "Admin"
            });

            // Act: Login with newly registered credentials
            var loginResult = await service.AuthenticateAsync(new LoginRequestDto
            {
                Username = uniqueUser,
                Password = password
            });

            // Assert
            Assert.NotNull(loginResult);
            Assert.NotEmpty(loginResult!.Token);
            Assert.Equal("Admin", loginResult.Role);
        }

        [Fact]
        public async Task RegisterAsync_WithDuplicateUsername_ThrowsArgumentException()
        {
            // Arrange
            var service = CreateService();
            var uniqueUser = $"dupuser_{Guid.NewGuid():N}";

            await service.RegisterAsync(new RegisterRequestDto
            {
                Username = uniqueUser,
                Email    = $"{uniqueUser}@example.com",
                Password = "Password123!"
            });

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.RegisterAsync(new RegisterRequestDto
                {
                    Username = uniqueUser,
                    Email    = $"other_{uniqueUser}@example.com",
                    Password = "DifferentPassword123!"
                })
            );
        }

        // ── LOGIN TESTS ──────────────────────────────────────────────────────────

        [Fact]
        public async Task AuthenticateAsync_WithValidAdminCredentials_ReturnsTokenWithAdminRole()
        {
            // Arrange
            var service = CreateService();
            var request = new LoginRequestDto
            {
                Username = "admin",
                Password = "Admin@123"
            };

            // Act
            var response = await service.AuthenticateAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.NotEmpty(response!.Token);
            Assert.Equal("admin", response.Username);
            Assert.Equal("Admin", response.Role);
            Assert.True(response.Expiration > DateTime.UtcNow);
        }

        [Fact]
        public async Task AuthenticateAsync_WithValidUserCredentials_ReturnsTokenWithUserRole()
        {
            // Arrange
            var service = CreateService();
            var request = new LoginRequestDto
            {
                Username = "user",
                Password = "User@123"
            };

            // Act
            var response = await service.AuthenticateAsync(request);

            // Assert
            Assert.NotNull(response);
            Assert.NotEmpty(response!.Token);
            Assert.Equal("user", response.Username);
            Assert.Equal("User", response.Role);
        }

        [Theory]
        [InlineData("admin", "WrongPassword!")]
        [InlineData("unknown_user", "AnyPassword")]
        [InlineData("", "")]
        public async Task AuthenticateAsync_WithInvalidCredentials_ReturnsNull(string username, string password)
        {
            // Arrange
            var service = CreateService();
            var request = new LoginRequestDto
            {
                Username = username,
                Password = password
            };

            // Act
            var response = await service.AuthenticateAsync(request);

            // Assert
            Assert.Null(response);
        }
    }
}
