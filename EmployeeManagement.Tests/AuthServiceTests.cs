using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;

namespace EmployeeManagement.Tests
{
    /// <summary>
    /// Day 10 — Authentication and Token Generation Unit Tests.
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
