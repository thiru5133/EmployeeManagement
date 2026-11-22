using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeManagement.API.Models
{
    /// <summary>
    /// Employee domain entity.
    ///
    /// Day 8 — EF Core annotations:
    ///   [Key]        → marks primary key (EF infers this for 'Id', but explicit is clearer)
    ///   [Required]   → maps to NOT NULL at the database level
    ///   [MaxLength]  → maps to NVARCHAR(n) instead of NVARCHAR(MAX) — better performance + index support
    ///   [Column]     → customise column type (e.g. DECIMAL precision for Salary)
    /// </summary>
    public class Employee
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string Department { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Salary { get; set; }

        public DateTime JoiningDate { get; set; }
    }
}
