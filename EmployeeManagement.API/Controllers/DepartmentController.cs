using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Controllers
{
    /// <summary>
    /// Day 9 — Department Controller demonstrating Entity Framework Core relationships:
    ///   - One-to-Many relationship (1 Department has many Employees)
    ///   - Eager loading using .Include(d => d.Employees)
    ///   - Tracking vs AsNoTracking for read queries
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DepartmentController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DepartmentController> _logger;

        public DepartmentController(AppDbContext context, ILogger<DepartmentController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/department
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DepartmentResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<DepartmentResponseDto>>> GetDepartments()
        {
            _logger.LogInformation("Fetching departments with eager-loaded employees.");

            var departments = await _context.Departments
                .Include(d => d.Employees) // Eager loading relationship
                .AsNoTracking()            // Read-only query optimization
                .ToListAsync();

            var dtos = departments.Select(d => new DepartmentResponseDto
            {
                Id            = d.Id,
                Name          = d.Name,
                Description   = d.Description,
                EmployeeCount = d.Employees.Count,
                Employees     = d.Employees.Select(e => new EmployeeResponseDto
                {
                    Id          = e.Id,
                    Name        = e.Name,
                    Email       = e.Email,
                    Department  = e.Department,
                    Salary      = e.Salary,
                    JoiningDate = e.JoiningDate
                })
            });

            return Ok(dtos);
        }

        // GET: api/department/1
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DepartmentResponseDto>> GetDepartmentById(int id)
        {
            var dept = await _context.Departments
                .Include(d => d.Employees)
                .AsNoTracking()
                .FirstOrDefaultAsync(d => d.Id == id);

            if (dept == null)
                return NotFound(new { message = $"Department with ID {id} was not found." });

            return Ok(new DepartmentResponseDto
            {
                Id            = dept.Id,
                Name          = dept.Name,
                Description   = dept.Description,
                EmployeeCount = dept.Employees.Count,
                Employees     = dept.Employees.Select(e => new EmployeeResponseDto
                {
                    Id          = e.Id,
                    Name        = e.Name,
                    Email       = e.Email,
                    Department  = e.Department,
                    Salary      = e.Salary,
                    JoiningDate = e.JoiningDate
                })
            });
        }

        // POST: api/department
        [HttpPost]
        [ProducesResponseType(typeof(DepartmentResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<DepartmentResponseDto>> CreateDepartment([FromBody] CreateDepartmentDto dto)
        {
            var dept = new Department
            {
                Name = dto.Name,
                Description = dto.Description
            };

            _context.Departments.Add(dept);
            await _context.SaveChangesAsync();

            var response = new DepartmentResponseDto
            {
                Id = dept.Id,
                Name = dept.Name,
                Description = dept.Description,
                EmployeeCount = 0
            };

            return CreatedAtAction(nameof(GetDepartmentById), new { id = dept.Id }, response);
        }
    }
}
