using EmployeeManagementSystem.DTOs;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Repositories;
using EmployeeManagementSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagementSystem.Services
{
    public interface IEmployeeService
    {
        Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync();
        Task<EmployeeResponseDto> GetEmployeeByIdAsync(int id);
        Task<int> CreateEmployeeAsync(CreateEmployeeDto dto);
        Task UpdateEmployeeAsync(int id, UpdateEmployeeDto dto);
        Task DeleteEmployeeAsync(int id);
        Task RotateShiftsAsync();
    }

    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        // Temporary injection of AppDbContext for complex queries like Include, 
        // until we implement Specifications in GenericRepository
        private readonly AppDbContext _context;

        public EmployeeService(IUnitOfWork unitOfWork, AppDbContext context)
        {
            _unitOfWork = unitOfWork;
            _context = context;
        }

        public async Task<IEnumerable<EmployeeResponseDto>> GetAllEmployeesAsync()
        {
            return await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Projects)
                .Select(e => new EmployeeResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Salary = e.Salary,
                    Email = e.Email,
                    DepartmentName = e.Department != null ? e.Department.Name : "Unassigned",
                    Projects = e.Projects.Select(p => p.ProjectName).ToList(),
                    CurrentShift = e.CurrentShift
                })
                .ToListAsync();
        }

        public async Task<EmployeeResponseDto> GetEmployeeByIdAsync(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .Include(e => e.Projects)
                .Where(e => e.Id == id)
                .Select(e => new EmployeeResponseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Salary = e.Salary,
                    Email = e.Email,
                    DepartmentName = e.Department != null ? e.Department.Name : "Unassigned",
                    Projects = e.Projects.Select(p => p.ProjectName).ToList(),
                    CurrentShift = e.CurrentShift
                })
                .FirstOrDefaultAsync();

            if (employee == null) throw new Exception("Employee not found");
            return employee;
        }

        public async Task<int> CreateEmployeeAsync(CreateEmployeeDto dto)
        {
            if (dto.DepartmentId.HasValue)
            {
                var departmentExists = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId.Value);
                if (departmentExists == null) throw new Exception("Invalid DepartmentId");
            }

            var projects = (await _unitOfWork.Projects.GetAllAsync())
                .Where(p => dto.ProjectIds != null && dto.ProjectIds.Contains(p.Id))
                .ToList();

            var employee = new Employee
            {
                Name = dto.Name,
                Salary = dto.Salary,
                Email = dto.Email,
                DepartmentId = dto.DepartmentId,
                Projects = projects
            };

            await _unitOfWork.Employees.AddAsync(employee);
            await _unitOfWork.CompleteAsync();

            return employee.Id;
        }

        public async Task UpdateEmployeeAsync(int id, UpdateEmployeeDto dto)
        {
            var employee = await _context.Employees.Include(e => e.Projects).FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) throw new Exception("Employee not found");

            if (dto.DepartmentId.HasValue)
            {
                var departmentExists = await _unitOfWork.Departments.GetByIdAsync(dto.DepartmentId.Value);
                if (departmentExists == null) throw new Exception("Invalid DepartmentId");
            }

            employee.Name = dto.Name;
            employee.Salary = dto.Salary;
            employee.Email = dto.Email;
            employee.DepartmentId = dto.DepartmentId;

            var projects = (await _unitOfWork.Projects.GetAllAsync())
                .Where(p => dto.ProjectIds != null && dto.ProjectIds.Contains(p.Id))
                .ToList();

            employee.Projects = projects;

            _unitOfWork.Employees.Update(employee);
            await _unitOfWork.CompleteAsync();
        }

        public async Task DeleteEmployeeAsync(int id)
        {
            var employee = await _unitOfWork.Employees.GetByIdAsync(id);
            if (employee == null) throw new Exception("Employee not found");

            _unitOfWork.Employees.Remove(employee);
            _unitOfWork.Employees.Remove(employee);
            await _unitOfWork.CompleteAsync();
        }

        public async Task RotateShiftsAsync()
        {
            var allEmployees = await _unitOfWork.Employees.GetAllAsync();
            var shifts = new[] { "Morning", "Evening", "Night" };

            foreach (var employee in allEmployees)
            {
                string newShift = shifts[Random.Shared.Next(shifts.Length)];
                while (newShift == employee.CurrentShift)
                {
                    newShift = shifts[Random.Shared.Next(shifts.Length)];
                }

                employee.CurrentShift = newShift;
                _unitOfWork.Employees.Update(employee);
            }

            await _unitOfWork.CompleteAsync();
        }
    }
}

