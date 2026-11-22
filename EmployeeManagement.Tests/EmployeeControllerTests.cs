using EmployeeManagement.API.Controllers;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Services;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ReturnsExtensions;

namespace EmployeeManagement.Tests
{
    /// <summary>
    /// Day 7 Controller Tests — using NSubstitute to mock IEmployeeService.
    ///
    /// Why mocking matters:
    ///   Before (Day 6): controller tests used a static in-memory list — tests
    ///   shared state and affected each other.
    ///
    ///   Now (Day 7): We mock IEmployeeService using NSubstitute. Each test gets a
    ///   fresh mock with EXACTLY the data it specifies. No shared state. No DB needed.
    ///   This is true unit testing — we test only the controller logic in isolation.
    /// </summary>
    public class EmployeeControllerTests
    {
        // ── Helpers ─────────────────────────────────────────────────────────────

        private static EmployeeController CreateController(IEmployeeService service)
            => new(service);

        private static EmployeeResponseDto SampleEmployee(int id = 1) => new()
        {
            Id = id,
            Name = "Thiru",
            Email = "thiru@gmail.com",
            Department = "IT",
            Salary = 45000,
            JoiningDate = new DateTime(2024, 1, 15)
        };

        // ── GET ALL ──────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetEmployees_ReturnsOkWithList()
        {
            // Arrange
            var service = Substitute.For<IEmployeeService>();
            service.GetAllAsync().Returns(new List<EmployeeResponseDto>
            {
                SampleEmployee(1),
                SampleEmployee(2)
            });
            var controller = CreateController(service);

            // Act
            var result = await controller.GetEmployees();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<EmployeeResponseDto>>(ok.Value);
            Assert.Equal(2, list.Count());
        }

        [Fact]
        public async Task GetEmployees_WhenEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            var service = Substitute.For<IEmployeeService>();
            service.GetAllAsync().Returns(Enumerable.Empty<EmployeeResponseDto>());
            var controller = CreateController(service);

            // Act
            var result = await controller.GetEmployees();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var list = Assert.IsAssignableFrom<IEnumerable<EmployeeResponseDto>>(ok.Value);
            Assert.Empty(list);
        }

        // ── GET PAGED (Day 9) ────────────────────────────────────────────────────

        [Fact]
        public async Task GetPagedEmployees_ReturnsOkWithPagedResult()
        {
            // Arrange
            var parameters = new EmployeeQueryParameters { Page = 1, PageSize = 5 };
            var pagedResult = new PagedResult<EmployeeResponseDto>
            {
                Data = new List<EmployeeResponseDto> { SampleEmployee(1) },
                Page = 1,
                PageSize = 5,
                TotalCount = 1
            };

            var service = Substitute.For<IEmployeeService>();
            service.GetPagedAsync(parameters).Returns(pagedResult);
            var controller = CreateController(service);

            // Act
            var result = await controller.GetPagedEmployees(parameters);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var body = Assert.IsType<PagedResult<EmployeeResponseDto>>(ok.Value);
            Assert.Single(body.Data);
            Assert.Equal(1, body.TotalCount);
        }

        // ── GET BY ID ────────────────────────────────────────────────────────────

        [Fact]
        public async Task GetEmployeeById_WhenFound_ReturnsOk()
        {
            // Arrange
            var service = Substitute.For<IEmployeeService>();
            service.GetByIdAsync(1).Returns(SampleEmployee(1));
            var controller = CreateController(service);

            // Act
            var result = await controller.GetEmployeeById(1);

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<EmployeeResponseDto>(ok.Value);
            Assert.Equal(1, dto.Id);
            Assert.Equal("Thiru", dto.Name);
        }

        [Fact]
        public async Task GetEmployeeById_WhenNotFound_Returns404()
        {
            // Arrange
            var service = Substitute.For<IEmployeeService>();
            service.GetByIdAsync(999).ReturnsNull();
            var controller = CreateController(service);

            // Act
            var result = await controller.GetEmployeeById(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        // ── CREATE ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task CreateEmployee_WithValidDto_Returns201Created()
        {
            // Arrange
            var dto = new CreateEmployeeDto
            {
                Name = "Kavitha",
                Email = "kavitha@example.com",
                Department = "Engineering",
                Salary = 60000,
                JoiningDate = new DateTime(2024, 3, 1)
            };
            var created = new EmployeeResponseDto
            {
                Id = 3,
                Name = dto.Name,
                Email = dto.Email,
                Department = dto.Department,
                Salary = dto.Salary,
                JoiningDate = dto.JoiningDate
            };

            var service = Substitute.For<IEmployeeService>();
            service.CreateAsync(dto).Returns(created);
            var controller = CreateController(service);

            // Act
            var result = await controller.CreateEmployee(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);

            var body = Assert.IsType<EmployeeResponseDto>(createdResult.Value);
            Assert.Equal(3, body.Id);
            Assert.Equal("Kavitha", body.Name);
        }

        // ── UPDATE ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task UpdateEmployee_WhenFound_Returns204NoContent()
        {
            // Arrange
            var dto = new UpdateEmployeeDto
            {
                Name = "Thiru Updated",
                Email = "thiru.updated@example.com",
                Department = "IT",
                Salary = 55000,
                JoiningDate = new DateTime(2024, 1, 15)
            };

            var service = Substitute.For<IEmployeeService>();
            service.UpdateAsync(1, dto).Returns(true);
            var controller = CreateController(service);

            // Act
            var result = await controller.UpdateEmployee(1, dto);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdateEmployee_WhenNotFound_Returns404()
        {
            // Arrange
            var dto = new UpdateEmployeeDto
            {
                Name = "Ghost",
                Email = "ghost@example.com",
                Department = "None",
                Salary = 1000,
                JoiningDate = DateTime.UtcNow
            };

            var service = Substitute.For<IEmployeeService>();
            service.UpdateAsync(999, dto).Returns(false);
            var controller = CreateController(service);

            // Act
            var result = await controller.UpdateEmployee(999, dto);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ── DELETE ───────────────────────────────────────────────────────────────

        [Fact]
        public async Task DeleteEmployee_WhenFound_Returns204NoContent()
        {
            // Arrange
            var service = Substitute.For<IEmployeeService>();
            service.DeleteAsync(1).Returns(true);
            var controller = CreateController(service);

            // Act
            var result = await controller.DeleteEmployee(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteEmployee_WhenNotFound_Returns404()
        {
            // Arrange
            var service = Substitute.For<IEmployeeService>();
            service.DeleteAsync(999).Returns(false);
            var controller = CreateController(service);

            // Act
            var result = await controller.DeleteEmployee(999);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        // ── SERVICE CALLED ONCE ──────────────────────────────────────────────────

        [Fact]
        public async Task GetEmployeeById_CallsServiceExactlyOnce()
        {
            // Arrange
            var service = Substitute.For<IEmployeeService>();
            service.GetByIdAsync(1).Returns(SampleEmployee(1));
            var controller = CreateController(service);

            // Act
            await controller.GetEmployeeById(1);

            // Assert — verify the service was called exactly once with ID=1
            await service.Received(1).GetByIdAsync(1);
        }
    }
}
