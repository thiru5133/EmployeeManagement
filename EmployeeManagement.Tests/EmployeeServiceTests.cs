using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Repositories;
using EmployeeManagement.API.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace EmployeeManagement.Tests
{
    /// <summary>
    /// Day 8 — EmployeeService Unit Tests.
    ///
    /// We mock IEmployeeRepository using NSubstitute so the service tests:
    ///   ✅ Run without a real database
    ///   ✅ Are fast and fully isolated
    ///   ✅ Test only the service's mapping + orchestration logic
    ///
    /// NullLogger<T> is used so we don't need to mock ILogger.
    /// </summary>
    public class EmployeeServiceTests
    {
        // ── Helpers ─────────────────────────────────────────────────────────────

        private static EmployeeService CreateService(IEmployeeRepository repository)
            => new(repository, NullLogger<EmployeeService>.Instance);

        private static Employee SampleEntity(int id = 1) => new()
        {
            Id         = id,
            Name       = "Thiru",
            Email      = "thiru@gmail.com",
            Department = "IT",
            Salary     = 45000,
            JoiningDate = new DateTime(2024, 1, 15)
        };

        private static EmployeeResponseDto SampleDto(int id = 1) => new()
        {
            Id         = id,
            Name       = "Thiru",
            Email      = "thiru@gmail.com",
            Department = "IT",
            Salary     = 45000,
            JoiningDate = new DateTime(2024, 1, 15)
        };

        // ── GET ALL ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllAsync_ReturnsAllEmployeesAsDtos()
        {
            // Arrange
            var repo = Substitute.For<IEmployeeRepository>();
            repo.GetAllAsync().Returns(new List<Employee> { SampleEntity(1), SampleEntity(2) });
            var service = CreateService(repo);

            // Act
            var result = (await service.GetAllAsync()).ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Thiru", result[0].Name);
        }

        [Fact]
        public async Task GetAllAsync_WhenEmpty_ReturnsEmptyList()
        {
            // Arrange
            var repo = Substitute.For<IEmployeeRepository>();
            repo.GetAllAsync().Returns(Enumerable.Empty<Employee>());
            var service = CreateService(repo);

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        // ── GET BY ID ────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetByIdAsync_WhenFound_ReturnsMappedDto()
        {
            // Arrange
            var repo = Substitute.For<IEmployeeRepository>();
            repo.GetByIdAsync(1).Returns(SampleEntity(1));
            var service = CreateService(repo);

            // Act
            var result = await service.GetByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result!.Id);
            Assert.Equal("Thiru", result.Name);
            Assert.Equal("thiru@gmail.com", result.Email);
        }

        [Fact]
        public async Task GetByIdAsync_WhenNotFound_ReturnsNull()
        {
            // Arrange
            var repo = Substitute.For<IEmployeeRepository>();
            repo.GetByIdAsync(999).ReturnsNull();
            var service = CreateService(repo);

            // Act
            var result = await service.GetByIdAsync(999);

            // Assert
            Assert.Null(result);
        }

        // ── CREATE ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateAsync_CallsRepositoryAndReturnsMappedDto()
        {
            // Arrange
            var dto = new CreateEmployeeDto
            {
                Name       = "Kavitha",
                Email      = "kavitha@example.com",
                Department = "Engineering",
                Salary     = 60000,
                JoiningDate = new DateTime(2024, 3, 1)
            };

            var savedEntity = new Employee
            {
                Id         = 5,
                Name       = dto.Name,
                Email      = dto.Email,
                Department = dto.Department,
                Salary     = dto.Salary,
                JoiningDate = dto.JoiningDate
            };

            var repo = Substitute.For<IEmployeeRepository>();
            // When AddAsync is called with any Employee, return savedEntity
            repo.AddAsync(Arg.Any<Employee>()).Returns(savedEntity);
            var service = CreateService(repo);

            // Act
            var result = await service.CreateAsync(dto);

            // Assert
            Assert.Equal(5, result.Id);
            Assert.Equal("Kavitha", result.Name);
            await repo.Received(1).AddAsync(Arg.Any<Employee>());
        }

        // ── UPDATE ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateAsync_WhenFound_UpdatesAndReturnsTrue()
        {
            // Arrange
            var dto = new UpdateEmployeeDto
            {
                Name       = "Thiru Updated",
                Email      = "thiru.updated@example.com",
                Department = "IT",
                Salary     = 55000,
                JoiningDate = new DateTime(2024, 1, 15)
            };

            var repo = Substitute.For<IEmployeeRepository>();
            repo.GetTrackedByIdAsync(1).Returns(SampleEntity(1));
            var service = CreateService(repo);

            // Act
            var result = await service.UpdateAsync(1, dto);

            // Assert
            Assert.True(result);
            await repo.Received(1).UpdateAsync(Arg.Any<Employee>());
        }

        [Fact]
        public async Task UpdateAsync_WhenNotFound_ReturnsFalse()
        {
            // Arrange
            var dto = new UpdateEmployeeDto
            {
                Name       = "Ghost",
                Email      = "ghost@example.com",
                Department = "None",
                Salary     = 1000,
                JoiningDate = DateTime.UtcNow
            };

            var repo = Substitute.For<IEmployeeRepository>();
            repo.GetTrackedByIdAsync(999).ReturnsNull();
            var service = CreateService(repo);

            // Act
            var result = await service.UpdateAsync(999, dto);

            // Assert
            Assert.False(result);
            await repo.DidNotReceive().UpdateAsync(Arg.Any<Employee>());
        }

        // ── DELETE ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteAsync_WhenFound_DeletesAndReturnsTrue()
        {
            // Arrange
            var repo = Substitute.For<IEmployeeRepository>();
            repo.GetTrackedByIdAsync(1).Returns(SampleEntity(1));
            var service = CreateService(repo);

            // Act
            var result = await service.DeleteAsync(1);

            // Assert
            Assert.True(result);
            await repo.Received(1).DeleteAsync(Arg.Any<Employee>());
        }

        [Fact]
        public async Task DeleteAsync_WhenNotFound_ReturnsFalse()
        {
            // Arrange
            var repo = Substitute.For<IEmployeeRepository>();
            repo.GetTrackedByIdAsync(999).ReturnsNull();
            var service = CreateService(repo);

            // Act
            var result = await service.DeleteAsync(999);

            // Assert
            Assert.False(result);
            await repo.DidNotReceive().DeleteAsync(Arg.Any<Employee>());
        }
    }
}
