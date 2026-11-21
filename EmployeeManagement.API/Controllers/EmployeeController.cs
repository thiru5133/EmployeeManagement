using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagement.API.Controllers
{
    /// <summary>
    /// Employee API Controller — Day 7 version.
    ///
    /// THIN controller: only handles HTTP concerns (routing, status codes, request/response).
    /// All business logic + data access lives in IEmployeeService.
    ///
    /// Day 7 DI concept: We depend on IEmployeeService (interface),
    /// NOT on EmployeeService (concrete class). This means:
    ///   - We can swap the real implementation for a fake/mock in tests
    ///   - The controller doesn't care HOW data is fetched (EF, in-memory, API, etc.)
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        // Constructor injection — ASP.NET Core DI container provides IEmployeeService
        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET: api/employee
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmployeeResponseDto>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EmployeeResponseDto>>> GetEmployees()
        {
            var employees = await _employeeService.GetAllAsync();
            return Ok(employees);
        }

        // GET: api/employee/1
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmployeeResponseDto>> GetEmployeeById(int id)
        {
            var employee = await _employeeService.GetByIdAsync(id);

            if (employee == null)
                return NotFound(new { message = $"Employee with ID {id} was not found." });

            return Ok(employee);
        }

        // POST: api/employee
        [HttpPost]
        [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<EmployeeResponseDto>> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            var created = await _employeeService.CreateAsync(dto);

            return CreatedAtAction(nameof(GetEmployeeById), new { id = created.Id }, created);
        }

        // PUT: api/employee/1
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var updated = await _employeeService.UpdateAsync(id, dto);

            if (!updated)
                return NotFound(new { message = $"Employee with ID {id} was not found." });

            return NoContent();
        }

        // DELETE: api/employee/1
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var deleted = await _employeeService.DeleteAsync(id);

            if (!deleted)
                return NotFound(new { message = $"Employee with ID {id} was not found." });

            return NoContent();
        }
    }
}
