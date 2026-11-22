using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.Repositories;

namespace EmployeeManagement.API.Services
{
    /// <summary>
    /// Concrete implementation of IEmployeeService.
    ///
    /// Day 8 — Repository Pattern refactor:
    ///   EmployeeService no longer imports AppDbContext or EF Core.
    ///   All data access is delegated to IEmployeeRepository.
    ///
    ///   This means:
    ///     ✅ Service tests can mock IEmployeeRepository — no DB needed
    ///     ✅ Swapping DB provider only requires changing the repository
    ///     ✅ The service stays focused on business logic, not SQL plumbing
    ///
    /// DI Lifetime: Scoped — one instance per HTTP request.
    /// IEmployeeRepository is also Scoped, so no captive dependency.
    /// </summary>
    public class EmployeeService : IEmployeeService
    {
        private readonly IEmployeeRepository _repository;
        private readonly ILogger<EmployeeService> _logger;

        public EmployeeService(IEmployeeRepository repository, ILogger<EmployeeService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        // ── GET ALL ─────────────────────────────────────────────────────────────
        public async Task<IEnumerable<EmployeeResponseDto>> GetAllAsync()
        {
            _logger.LogInformation("Fetching all employees.");

            var employees = await _repository.GetAllAsync();
            var list = employees.ToList();

            _logger.LogInformation("Fetched {Count} employee(s).", list.Count);

            return list.Select(MapToDto);
        }

        // ── GET PAGED (Day 9) ────────────────────────────────────────────────────
        public async Task<PagedResult<EmployeeResponseDto>> GetPagedAsync(EmployeeQueryParameters parameters)
        {
            _logger.LogInformation("Fetching paged employees. Page={Page}, Size={Size}, Dept={Dept}, Term={Term}",
                parameters.Page, parameters.PageSize, parameters.Department, parameters.SearchTerm);

            var pagedEntities = await _repository.GetPagedAsync(parameters);

            return new PagedResult<EmployeeResponseDto>
            {
                Data       = pagedEntities.Data.Select(MapToDto),
                Page       = pagedEntities.Page,
                PageSize   = pagedEntities.PageSize,
                TotalCount = pagedEntities.TotalCount
            };
        }

        // ── GET BY ID ───────────────────────────────────────────────────────────
        public async Task<EmployeeResponseDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("Fetching employee with ID {Id}.", id);

            var employee = await _repository.GetByIdAsync(id);

            if (employee is null)
            {
                _logger.LogWarning("Employee with ID {Id} was NOT found.", id);
                return null;
            }

            _logger.LogInformation("Employee {Id} found: {Name}.", id, employee.Name);
            return MapToDto(employee);
        }

        // ── CREATE ──────────────────────────────────────────────────────────────
        public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeDto dto)
        {
            _logger.LogInformation("Creating employee: {Name} ({Email}).", dto.Name, dto.Email);

            var employee = new Employee
            {
                Name       = dto.Name,
                Email      = dto.Email,
                Department = dto.Department,
                Salary     = dto.Salary,
                JoiningDate = dto.JoiningDate
            };

            var created = await _repository.AddAsync(employee);

            _logger.LogInformation("Employee created with ID {Id}.", created.Id);
            return MapToDto(created);
        }

        // ── UPDATE ──────────────────────────────────────────────────────────────
        public async Task<bool> UpdateAsync(int id, UpdateEmployeeDto dto)
        {
            _logger.LogInformation("Updating employee with ID {Id}.", id);

            // We need a tracked entity for EF to detect changes, so fetch with FindAsync
            // Note: cannot use repository's AsNoTracking version here
            var employee = await _repository.GetTrackedByIdAsync(id);

            if (employee is null)
            {
                _logger.LogWarning("Update failed — employee {Id} not found.", id);
                return false;
            }

            employee.Name       = dto.Name;
            employee.Email      = dto.Email;
            employee.Department = dto.Department;
            employee.Salary     = dto.Salary;
            employee.JoiningDate = dto.JoiningDate;

            await _repository.UpdateAsync(employee);

            _logger.LogInformation("Employee {Id} updated successfully.", id);
            return true;
        }

        // ── DELETE ──────────────────────────────────────────────────────────────
        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("Deleting employee with ID {Id}.", id);

            var employee = await _repository.GetTrackedByIdAsync(id);

            if (employee is null)
            {
                _logger.LogWarning("Delete failed — employee {Id} not found.", id);
                return false;
            }

            await _repository.DeleteAsync(employee);

            _logger.LogInformation("Employee {Id} deleted successfully.", id);
            return true;
        }

        // ── MAPPER ──────────────────────────────────────────────────────────────
        private static EmployeeResponseDto MapToDto(Employee e) => new()
        {
            Id         = e.Id,
            Name       = e.Name,
            Email      = e.Email,
            Department = e.Department,
            Salary     = e.Salary,
            JoiningDate = e.JoiningDate
        };
    }
}
