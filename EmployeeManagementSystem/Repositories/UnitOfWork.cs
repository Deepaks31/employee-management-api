using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Repositories;
using EmployeeManagementSystem.Repositories;

namespace EmployeeManagementSystem.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public IGenericRepository<Employee> Employees { get; private set; }
        public IGenericRepository<Department> Departments { get; private set; }
        public IGenericRepository<Project> Projects { get; private set; }
        public IGenericRepository<User> Users { get; private set; }

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            Employees = new GenericRepository<Employee>(_context);
            Departments = new GenericRepository<Department>(_context);
            Projects = new GenericRepository<Project>(_context);
            Users = new GenericRepository<User>(_context);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}

