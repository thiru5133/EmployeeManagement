
using EmployeeManagement.API.Data;
using EmployeeManagement.API.DTOs;
using EmployeeManagement.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly AppDbContext _context;

        // Constructor Dependency Injection
        public EmployeeController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Employee
        // Get all employees
        [HttpGet]
        public async Task<ActionResult<List<Employee>>> GetEmployees()
        {
            var employees = await _context.Employees.ToListAsync();

            return Ok(employees);
        }

        // GET: api/Employee/1
        // Get employee by ID
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Employee>> GetEmployeeById(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound($"Employee with ID {id} was not found.");
            }

            return Ok(employee);
        }

        // POST: api/Employee
        // Create new employee
        [HttpPost]
        public async Task<ActionResult<Employee>> CreateEmployee(
            [FromBody] CreateEmployeeRequestDto employeeDto)
        {
            var employee = new Employee
            {
                Name = employeeDto.Name,
                Email = employeeDto.Email,
                Department = employeeDto.Department,
                Salary = employeeDto.Salary,
                JoiningDate = employeeDto.JoiningDate
            };

            _context.Employees.Add(employee);

            await _context.SaveChangesAsync();
            
            // Convert Employee entity into response DTO
            var response = new EmployeeResponseDto
            {
                Id = employee.Id,
                Name = employee.Name,
                Email = employee.Email,
                Department = employee.Department,
                Salary = employee.Salary,
                JoiningDate = employee.JoiningDate
            };

            return CreatedAtAction(
                nameof(GetEmployeeById),
                new { id = employee.Id },
                response
            );
        }
        
        // PUT: api/Employee/1
        // Update existing employee
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateEmployee(
            int id,
            [FromBody] Employee updatedEmployee)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound($"Employee with ID {id} was not found.");
            }

            employee.Name = updatedEmployee.Name;
            employee.Email = updatedEmployee.Email;
            employee.Department = updatedEmployee.Department;
            employee.Salary = updatedEmployee.Salary;
            employee.JoiningDate = updatedEmployee.JoiningDate;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/Employee/1
        // Delete employee
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);

            if (employee == null)
            {
                return NotFound($"Employee with ID {id} was not found.");
            }

            _context.Employees.Remove(employee);

            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
