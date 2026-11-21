using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Services
{
    /// <summary>
    /// Contract for all Employee business operations.
    /// The controller depends on this interface, NOT on EF Core directly.
    /// This makes the code testable and loosely coupled (Day 7 - DI).
    /// </summary>
    public interface IEmployeeService
    {
        /// <summary>Returns all employees as response DTOs.</summary>
        Task<IEnumerable<EmployeeResponseDto>> GetAllAsync();

        /// <summary>Returns a single employee by ID, or null if not found.</summary>
        Task<EmployeeResponseDto?> GetByIdAsync(int id);

        /// <summary>Creates a new employee and returns the created record.</summary>
        Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto);

        /// <summary>Updates an existing employee. Returns true if found and updated, false if not found.</summary>
        Task<bool> UpdateAsync(int id, UpdateEmployeeDto dto);

        /// <summary>Deletes an employee. Returns true if found and deleted, false if not found.</summary>
        Task<bool> DeleteAsync(int id);
    }
}
