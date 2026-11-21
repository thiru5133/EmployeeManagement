
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs
{
    public class CreateEmployeeRequestDto
    {
        [Required(ErrorMessage = "Employee name is required.")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Enter a valid email address.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required.")]
        public string Department { get; set; } = string.Empty;

        [Range(1, 10000000, ErrorMessage = "Salary must be greater than zero.")]
        public decimal Salary { get; set; }

        [Required]
        public DateTime JoiningDate { get; set; }
    }
}
