using EmployeeManagementSystem.DTOs;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        // GET ALL EMPLOYEES
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _employeeService.GetAllEmployeesAsync();
            return Ok(employees);
        }

        // GET EMPLOYEE BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployee(int id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            return Ok(employee);
        }

        // CREATE EMPLOYEE
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeDto dto)
        {
            var employeeId = await _employeeService.CreateEmployeeAsync(dto);
            return Ok(new
            {
                Message = "Employee created successfully",
                EmployeeId = employeeId
            });
        }

        // UPDATE EMPLOYEE
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, UpdateEmployeeDto dto)
        {
            await _employeeService.UpdateEmployeeAsync(id, dto);
            return Ok(new { Message = "Employee updated successfully" });
        }

        // DELETE EMPLOYEE
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            await _employeeService.DeleteEmployeeAsync(id);
            return Ok(new { Message = "Employee deleted successfully" });
        }

        // ROTATE SHIFTS (M2M Daemon Only)
        [Authorize(AuthenticationSchemes = "AzureAd", Roles = "Daemon.Execute")]
        [HttpPost("rotate-shifts")]
        public async Task<IActionResult> RotateShifts()
        {
            await _employeeService.RotateShiftsAsync();
            return Ok(new { Message = "Shifts rotated successfully via M2M Authentication" });
        }
    }
}

