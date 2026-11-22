using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs
{
    public class CreateDepartmentDto
    {
        [Required(ErrorMessage = "Department name is required.")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Department name must be between 2 and 50 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters.")]
        public string Description { get; set; } = string.Empty;
    }

    public class DepartmentResponseDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int EmployeeCount { get; set; }
        public IEnumerable<EmployeeResponseDto> Employees { get; set; } = Enumerable.Empty<EmployeeResponseDto>();
    }
}
