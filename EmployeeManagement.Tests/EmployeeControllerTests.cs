using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.API.Controllers;
using EmployeeManagement.API.DTOs;
using Xunit;

namespace EmployeeManagement.Tests
{
    public class EmployeeControllerTests
    {
        // We need a fresh controller for each test - but the static list is shared.
        // Note: in Day 8 when we introduce Service layer, this will be much cleaner.
        // For Day 6, we test the controller's HTTP response shapes.

        [Fact]
        public void GetEmployees_ReturnsOkResult()
        {
            // Arrange
            var controller = new EmployeeController();

            // Act
            var result = controller.GetEmployees();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.NotNull(okResult.Value);
            Assert.Equal(200, okResult.StatusCode);
        }

        [Fact]
        public void GetEmployeeById_WithValidId_ReturnsOkResult()
        {
            // Arrange
            var controller = new EmployeeController();

            // Act
            var result = controller.GetEmployeeById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<EmployeeResponseDto>(okResult.Value);
            Assert.Equal(1, dto.Id);
            // Note: static list is shared across tests - we just verify ID returned, not Name
            // (In Day 8, service layer injection will give us proper test isolation)
            Assert.NotNull(dto.Name);
        }

        [Fact]
        public void GetEmployeeById_WithNonExistentId_Returns404NotFound()
        {
            // Arrange
            var controller = new EmployeeController();

            // Act
            var result = controller.GetEmployeeById(9999);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Equal(404, notFoundResult.StatusCode);
        }

        [Fact]
        public void CreateEmployee_WithValidDto_Returns201Created()
        {
            // Arrange
            var controller = new EmployeeController();
            var dto = new CreateEmployeeDto
            {
                Name = "New Employee",
                Email = "new@example.com",
                Department = "Sales",
                Salary = 42000,
                JoiningDate = DateTime.UtcNow
            };

            // Act
            var result = controller.CreateEmployee(dto);

            // Assert
            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);

            var created = Assert.IsType<EmployeeResponseDto>(createdResult.Value);
            Assert.Equal("New Employee", created.Name);
            Assert.Equal("new@example.com", created.Email);
            Assert.True(created.Id > 0);
        }

        [Fact]
        public void UpdateEmployee_WithValidDto_Returns204NoContent()
        {
            // Arrange
            var controller = new EmployeeController();
            var dto = new UpdateEmployeeDto
            {
                Name = "Thiru Updated",
                Email = "thiru.updated@example.com",
                Department = "IT",
                Salary = 55000,
                JoiningDate = new DateTime(2024, 1, 15)
            };

            // Act
            var result = controller.UpdateEmployee(1, dto);

            // Assert
            var noContent = Assert.IsType<NoContentResult>(result);
            Assert.Equal(204, noContent.StatusCode);
        }

        [Fact]
        public void UpdateEmployee_WithNonExistentId_Returns404NotFound()
        {
            // Arrange
            var controller = new EmployeeController();
            var dto = new UpdateEmployeeDto
            {
                Name = "Ghost",
                Email = "ghost@example.com",
                Department = "None",
                Salary = 1000,
                JoiningDate = DateTime.UtcNow
            };

            // Act
            var result = controller.UpdateEmployee(9999, dto);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }

        [Fact]
        public void DeleteEmployee_WithNonExistentId_Returns404NotFound()
        {
            // Arrange
            var controller = new EmployeeController();

            // Act
            var result = controller.DeleteEmployee(9999);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal(404, notFound.StatusCode);
        }
    }
}
