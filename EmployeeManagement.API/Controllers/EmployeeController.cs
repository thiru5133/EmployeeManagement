using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.API.Models;
using EmployeeManagement.API.DTOs;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class EmployeeController : ControllerBase
    {
        // Temporary in-memory employee data (for Day 6)
        private static readonly List<Employee> employees = new()
        {
            new Employee
            {
                Id = 1,
                Name = "Thiru",
                Email = "thiru@gmail.com",
                Department = "IT",
                Salary = 45000,
                JoiningDate = new DateTime(2024, 1, 15)
            },
            new Employee
            {
                Id = 2,
                Name = "Arun",
                Email = "arun@gmail.com",
                Department = "HR",
                Salary = 35000,
                JoiningDate = new DateTime(2023, 6, 10)
            }
        };

        // GET: api/employee
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<EmployeeResponseDto>), StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<EmployeeResponseDto>> GetEmployees()
        {
            var response = employees.Select(MapToResponseDto);
            return Ok(response);
        }

        // GET: api/employee/1
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<EmployeeResponseDto> GetEmployeeById(int id)
        {
            var employee = employees.FirstOrDefault(e => e.Id == id);
            if (employee == null)
            {
                return NotFound(new { message = $"Employee with ID {id} was not found." });
            }

            return Ok(MapToResponseDto(employee));
        }

        // POST: api/employee
        [HttpPost]
        [ProducesResponseType(typeof(EmployeeResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<EmployeeResponseDto> CreateEmployee([FromBody] CreateEmployeeDto dto)
        {
            var nextId = employees.Count > 0
                ? employees.Max(e => e.Id) + 1
                : 1;

            var employee = new Employee
            {
                Id = nextId,
                Name = dto.Name,
                Email = dto.Email,
                Department = dto.Department,
                Salary = dto.Salary,
                JoiningDate = dto.JoiningDate
            };

            employees.Add(employee);

            var response = MapToResponseDto(employee);

            return CreatedAtAction(
                nameof(GetEmployeeById),
                new { id = employee.Id },
                response
            );
        }

        // PUT: api/employee/1
        [HttpPut("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateEmployee(int id, [FromBody] UpdateEmployeeDto dto)
        {
            var employee = employees.FirstOrDefault(e => e.Id == id);
            if (employee == null)
            {
                return NotFound(new { message = $"Employee with ID {id} was not found." });
            }

            employee.Name = dto.Name;
            employee.Email = dto.Email;
            employee.Department = dto.Department;
            employee.Salary = dto.Salary;
            employee.JoiningDate = dto.JoiningDate;

            return NoContent();
        }

        // DELETE: api/employee/1
        [HttpDelete("{id:int}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteEmployee(int id)
        {
            var employee = employees.FirstOrDefault(e => e.Id == id);
            if (employee == null)
            {
                return NotFound(new { message = $"Employee with ID {id} was not found." });
            }

            employees.Remove(employee);
            return NoContent();
        }

        private static EmployeeResponseDto MapToResponseDto(Employee employee)
        {
            return new EmployeeResponseDto
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
}
