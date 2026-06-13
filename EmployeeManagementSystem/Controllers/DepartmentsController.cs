using EmployeeManagementSystem.DTOs;
using EmployeeManagementSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EmployeeManagementSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class DepartmentsController : ControllerBase
    {
        private readonly IDepartmentService _departmentService;

        public DepartmentsController(IDepartmentService departmentService)
        {
            _departmentService = departmentService;
        }

        // GET ALL DEPARTMENTS
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _departmentService.GetAllDepartmentsAsync();
            return Ok(departments);
        }

        // GET DEPARTMENT BY ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetDepartment(int id)
        {
            var department = await _departmentService.GetDepartmentByIdAsync(id);
            return Ok(department);
        }

        // CREATE DEPARTMENT
        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> CreateDepartment(CreateDepartmentDto dto)
        {
            var department = await _departmentService.CreateDepartmentAsync(dto);
            return Ok(department);
        }

        // UPDATE DEPARTMENT
        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDepartment(int id, UpdateDepartmentDto dto)
        {
            var department = await _departmentService.UpdateDepartmentAsync(id, dto);
            return Ok(department);
        }

        // DELETE DEPARTMENT
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDepartment(int id)
        {
            await _departmentService.DeleteDepartmentAsync(id);
            return Ok(new { Message = "Department deleted successfully" });
        }
    }
}

