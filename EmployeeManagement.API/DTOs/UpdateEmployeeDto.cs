using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.API.DTOs
{
    public class UpdateEmployeeDto
    {
        [Required(ErrorMessage = "Name is required")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address format")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Department is required")]
        [StringLength(30, MinimumLength = 2, ErrorMessage = "Department must be between 2 and 30 characters")]
        public string Department { get; set; } = string.Empty;

        [Range(1000, 1000000, ErrorMessage = "Salary must be between 1,000 and 1,000,000")]
        public decimal Salary { get; set; }

        [Required(ErrorMessage = "Joining date is required")]
        public DateTime JoiningDate { get; set; }
    }
}
