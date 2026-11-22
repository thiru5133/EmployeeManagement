using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EmployeeManagement.API.Models
{
    /// <summary>
    /// Day 9 — Department entity establishing a 1-to-many relationship with Employee.
    /// One Department has many Employees.
    /// </summary>
    public class Department
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [JsonIgnore]
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
