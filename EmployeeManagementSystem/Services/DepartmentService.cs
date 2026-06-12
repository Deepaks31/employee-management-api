using EmployeeManagementSystem.DTOs;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Repositories;

namespace EmployeeManagementSystem.Services
{
    public interface IDepartmentService
    {
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();
        Task<Department> GetDepartmentByIdAsync(int id);
        Task<Department> CreateDepartmentAsync(CreateDepartmentDto dto);
        Task<Department> UpdateDepartmentAsync(int id, UpdateDepartmentDto dto);
        Task DeleteDepartmentAsync(int id);
    }

    public class DepartmentService : IDepartmentService
    {
        private readonly IUnitOfWork _unitOfWork;

        public DepartmentService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Department>> GetAllDepartmentsAsync()
        {
            // Here you can also include Employees using eager loading if needed,
            // but in a Generic Repository you might need a custom method or specification pattern.
            // For now, we return basic departments.
            return await _unitOfWork.Departments.GetAllAsync();
        }

        public async Task<Department> GetDepartmentByIdAsync(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null) throw new Exception("Department not found");
            return department;
        }

        public async Task<Department> CreateDepartmentAsync(CreateDepartmentDto dto)
        {
            var department = new Department { Name = dto.Name };
            await _unitOfWork.Departments.AddAsync(department);
            await _unitOfWork.CompleteAsync();
            return department;
        }

        public async Task<Department> UpdateDepartmentAsync(int id, UpdateDepartmentDto dto)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null) throw new Exception("Department not found");
            
            department.Name = dto.Name;
            _unitOfWork.Departments.Update(department);
            await _unitOfWork.CompleteAsync();
            
            return department;
        }

        public async Task DeleteDepartmentAsync(int id)
        {
            var department = await _unitOfWork.Departments.GetByIdAsync(id);
            if (department == null) throw new Exception("Department not found");

            _unitOfWork.Departments.Remove(department);
            await _unitOfWork.CompleteAsync();
        }
    }
}

