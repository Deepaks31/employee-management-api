using EmployeeManagementSystem.Models;

namespace EmployeeManagementSystem.Repositories
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<Employee> Employees { get; }
        IGenericRepository<Department> Departments { get; }
        IGenericRepository<Project> Projects { get; }
        IGenericRepository<User> Users { get; }
        Task<int> CompleteAsync();
    }
}

