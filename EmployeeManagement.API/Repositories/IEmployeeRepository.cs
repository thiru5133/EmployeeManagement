using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Repositories
{
    /// <summary>
    /// Repository contract for Employee data access.
    ///
    /// Day 8 — Repository Pattern:
    ///   The repository sits between the Service layer and EF Core.
    ///   The Service layer depends on this interface, NOT on AppDbContext directly.
    ///
    ///   Benefits:
    ///     ✅ Service tests can mock IEmployeeRepository (no real DB needed)
    ///     ✅ Swapping the data store (EF → Dapper → API) only requires a new implementation
    ///     ✅ Single Responsibility — DbContext details stay inside the repository
    ///
    ///   Layering:
    ///     Controller → IEmployeeService → IEmployeeRepository → AppDbContext → SQL Server
    /// </summary>
    public interface IEmployeeRepository
    {
        /// <summary>Returns all employees (read-only, no change tracking).</summary>
        Task<IEnumerable<Employee>> GetAllAsync();

        /// <summary>
        /// Day 9: Returns a paginated, filtered, and sorted list of employees.
        /// Demonstrates LINQ-to-database queries with AsNoTracking.
        /// </summary>
        Task<PagedResult<Employee>> GetPagedAsync(EmployeeQueryParameters parameters);

        /// <summary>Returns a single employee by primary key, or null if not found.</summary>
        Task<Employee?> GetByIdAsync(int id);

        /// <summary>Adds a new employee and persists the change.</summary>
        Task<Employee> AddAsync(Employee employee);

        /// <summary>Persists changes made to an already-tracked employee entity.</summary>
        Task UpdateAsync(Employee employee);

        /// <summary>Removes the employee and persists the change.</summary>
        Task DeleteAsync(Employee employee);

        /// <summary>Returns true if an employee with the given ID exists.</summary>
        Task<bool> ExistsAsync(int id);

        /// <summary>
        /// Returns a single tracked employee entity by ID (for Update/Delete operations).
        /// Unlike GetByIdAsync, this does NOT use AsNoTracking — EF must track the entity
        /// so that property changes are detected on SaveChanges.
        /// </summary>
        Task<Employee?> GetTrackedByIdAsync(int id);
    }
}
