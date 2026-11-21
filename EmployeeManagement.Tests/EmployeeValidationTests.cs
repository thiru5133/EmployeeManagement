using System.ComponentModel.DataAnnotations;
using EmployeeManagement.API.DTOs;
using Xunit;

namespace EmployeeManagement.Tests
{
    public class EmployeeValidationTests
    {
        private static IList<ValidationResult> ValidateModel(object model)
        {
            var results = new List<ValidationResult>();
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            Validator.TryValidateObject(model, context, results, validateAllProperties: true);
            return results;
        }

        [Fact]
        public void CreateEmployeeDto_WithValidData_PassesValidation()
        {
            // Arrange
            var dto = new CreateEmployeeDto
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Department = "Engineering",
                Salary = 50000,
                JoiningDate = DateTime.UtcNow
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            Assert.Empty(validationResults);
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public void CreateEmployeeDto_WithMissingOrEmptyName_FailsValidation(string? invalidName)
        {
            // Arrange
            var dto = new CreateEmployeeDto
            {
                Name = invalidName!,
                Email = "john.doe@example.com",
                Department = "Engineering",
                Salary = 50000,
                JoiningDate = DateTime.UtcNow
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateEmployeeDto.Name)));
        }

        [Theory]
        [InlineData("not-an-email")]
        [InlineData("nodomain")]
        [InlineData("@nodomain.com")]
        public void CreateEmployeeDto_WithInvalidEmail_FailsValidation(string invalidEmail)
        {
            // Arrange
            var dto = new CreateEmployeeDto
            {
                Name = "John Doe",
                Email = invalidEmail,
                Department = "Engineering",
                Salary = 50000,
                JoiningDate = DateTime.UtcNow
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateEmployeeDto.Email)));
        }

        [Theory]
        [InlineData(500)]       // below minimum (1000)
        [InlineData(-100)]      // negative
        [InlineData(2000000)]   // above maximum (1000000)
        public void CreateEmployeeDto_WithSalaryOutOfRange_FailsValidation(decimal invalidSalary)
        {
            // Arrange
            var dto = new CreateEmployeeDto
            {
                Name = "John Doe",
                Email = "john.doe@example.com",
                Department = "Engineering",
                Salary = invalidSalary,
                JoiningDate = DateTime.UtcNow
            };

            // Act
            var validationResults = ValidateModel(dto);

            // Assert
            Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(CreateEmployeeDto.Salary)));
        }
    }
}
