using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Services
{
    /// <summary>
    /// Concrete implementation of IEmployeeService.
    ///
    /// Day 7 DI concept:
    ///   - Registered as SCOPED → one instance per HTTP request
    ///   - AppDbContext is also Scoped, so this is safe (no captive dependency)
    ///   - ILogger<T> is injected by the DI container automatically
    ///
    /// Why Scoped and not Singleton?
    ///   AppDbContext tracks entity state per-request. A Singleton service holding a
    ///   Scoped DbContext would cause the context to outlive the request (captive
    ///   dependency bug) — leading to stale data or threading issues.
    /// </summary>
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(AppDbContext context, ILogger<EmployeeService> logger)
        {
            _context = context;
            _logger = logger;
        }

        // ── GET ALL ─────────────────────────────────────────────────────────────
        public async Task<IEnumerable<EmployeeResponseDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all employees from database.");

            var employees = await _context.Employees
                .AsNoTracking()   // read-only query → no change tracking overhead
                .ToListAsync();

            _logger.LogInformation("Fetched {Count} employee(s).", employees.Count);

            return employees.Select(MapToDto);
        }

        // ── GET BY ID ───────────────────────────────────────────────────────────
        public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching employee with ID {Id}.", id);

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == id);

            if (employee == null)
            {
                _logger.LogWarning("Employee with ID {Id} was NOT found.", id);
                return null;
            }

            _logger.LogInformation("Employee with ID {Id} found: {Name}.", id, employee.Name);
            return MapToDto(employee);
        }

        // ── CREATE ──────────────────────────────────────────────────────────────
        public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto)
        {
            _logger.LogInformation("Creating new employee: {Name} ({Email}).", dto.Name, dto.Email);

            var employee = new Employee
            {
                Name = dto.Name,
                Email = dto.Email,
                Department = dto.Department,
                Salary = dto.Salary,
                JoiningDate = dto.JoiningDate
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee created successfully with ID {Id}.", employee.Id);

            return MapToDto(employee);
        }

        // ── UPDATE ──────────────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            _logger.LogInformation("Updating employee with ID {Id}.", id);

            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                _logger.LogWarning("Update failed — employee with ID {Id} was NOT found.", id);
                return false;
            }

            employee.Name = dto.Name;
            employee.Email = dto.Email;
            employee.Department = dto.Department;
            employee.Salary = dto.Salary;
            employee.JoiningDate = dto.JoiningDate;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee with ID {Id} updated successfully.", id);
            return true;
        }

        // ── DELETE ──────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting employee with ID {Id}.", id);

            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                _logger.LogWarning("Delete failed — employee with ID {Id} was NOT found.", id);
                return false;
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Employee with ID {Id} deleted successfully.", id);
            return true;
        }

        // ── MAPPER ──────────────────────────────────────────────────────────────
        private static EmployeeResponseDto MapToDto(Employee employee) => new()
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            Department = employee.Department,
            Salary = employee.Salary,
            JoiningDate = employee.JoiningDate
        };
    }
}
