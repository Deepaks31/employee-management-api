using EmployeeManagementSystem.DTOs;
using EmployeeManagementSystem.Services;
using EmployeeManagementSystem.Models;
using EmployeeManagementSystem.Repositories;
using EmployeeManagementSystem.Security;

namespace EmployeeManagementSystem.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IJwtTokenGenerator _jwtTokenGenerator;

        public AuthService(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher, IJwtTokenGenerator jwtTokenGenerator)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _jwtTokenGenerator = jwtTokenGenerator;
        }

        public async Task<string> RegisterAsync(RegisterDto dto)
        {
            var existingUsers = await _unitOfWork.Users.FindAsync(u => u.Username == dto.Username);
            if (existingUsers.Any())
            {
                throw new Exception("User already exists");
            }

            var user = new User
            {
                Username = dto.Username,
                Password = _passwordHasher.HashPassword(dto.Password),
                Role = dto.Role
            };

            await _unitOfWork.Users.AddAsync(user);

            if (dto.Role == "Employee")
            {
                var employee = new Employee
                {
                    Name = dto.Username,
                    Email = "Update Email",
                    Salary = 0,
                    DepartmentId = null,
                    Projects = new List<Project>()
                };
                await _unitOfWork.Employees.AddAsync(employee);
            }

            await _unitOfWork.CompleteAsync();

            return "User registered successfully";
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            var users = await _unitOfWork.Users.FindAsync(u => u.Username == dto.Username);
            var user = users.FirstOrDefault();

            if (user == null || !_passwordHasher.VerifyPassword(dto.Password, user.Password))
            {
                throw new Exception("Invalid username or password");
            }

            return _jwtTokenGenerator.GenerateToken(user);
        }
    }
}

