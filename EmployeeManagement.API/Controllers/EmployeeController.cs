
using Microsoft.AspNetCore.Mvc;
using EmployeeManagement.API.Models;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        // Temporary employee data (in-memory)
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
        public ActionResult<List<Employee>> GetEmployees()
        {
            return Ok(employees);
        }


        // GET: api/Employee/1
        [HttpGet("{id:int}")]
        public ActionResult<Employee> GetEmployeeById(int id)
        {
            var employee = employees.FirstOrDefault(e => e.Id == id);

            if (employee == null)
            {
                return NotFound($"Employee with ID {id} was not found.");
            }

            return Ok(employee);
        }


        // POST: api/Employee
        [HttpPost]
        public ActionResult<Employee> CreateEmployee([FromBody] Employee employee)
        {
            // Generate the next employee ID
            employee.Id = employees.Count > 0
                ? employees.Max(e => e.Id) + 1
                : 1;

            // Add employee to the list
            employees.Add(employee);

            // Return HTTP 201 Created
            return CreatedAtAction(
                nameof(GetEmployeeById),
                new { id = employee.Id },
                employee
            );
        }


        // PUT: api/Employee/3
        [HttpPut("{id:int}")]
        public IActionResult UpdateEmployee(int id, [FromBody] Employee updatedEmployee)
        {
            // Find existing employee
            var employee = employees.FirstOrDefault(e => e.Id == id);

            // Check whether employee exists
            if (employee == null)
            {
                return NotFound($"Employee with ID {id} was not found.");
            }

            // Update employee properties
            employee.Name = updatedEmployee.Name;
            employee.Email = updatedEmployee.Email;
            employee.Department = updatedEmployee.Department;
            employee.Salary = updatedEmployee.Salary;
            employee.JoiningDate = updatedEmployee.JoiningDate;

            return NoContent();
        }

        // DELETE: api/Employee/3
        [HttpDelete("{id:int}")]
        public IActionResult DeleteEmployee(int id)
        {
            var employee = employees.FirstOrDefault(e => e.Id == id);

            if (employee == null)
            {
                return NotFound($"Employee with ID {id} was not found.");
            }

            employees.Remove(employee);

            return NoContent();
        }

    }
}
