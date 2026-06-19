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
                Role = "Employee" // Default role for all new signups
            };

            await _unitOfWork.Users.AddAsync(user);

            // Always create an employee profile since role is Employee
            var employee = new Employee
            {
                Name = dto.Username,
                Email = dto.Email, // Map the provided email
                Salary = 0,
                DepartmentId = null,
                Projects = new List<Project>()
            };
            await _unitOfWork.Employees.AddAsync(employee);

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

        public async Task<string> SsoLoginAsync(SsoLoginDto dto)
        {
            using var httpClient = new System.Net.Http.HttpClient();
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", dto.Token);
            
            var response = await httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception("Invalid Microsoft SSO Token");
            }

            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = System.Text.Json.JsonDocument.Parse(content);
            var root = jsonDoc.RootElement;
            
            string email = root.TryGetProperty("userPrincipalName", out var upnProp) ? upnProp.GetString() : null;
            if (string.IsNullOrEmpty(email))
            {
                email = root.TryGetProperty("mail", out var mailProp) ? mailProp.GetString() : null;
            }

            if (string.IsNullOrEmpty(email) || !email.EndsWith("@eigensecure.com", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Access Denied: You must use an @eigensecure.com account.");
            }

            var username = email;
            var users = await _unitOfWork.Users.FindAsync(u => u.Username == username);
            var user = users.FirstOrDefault();
            
            bool changesMade = false;

            if (user == null)
            {
                // JIT Provisioning for User
                user = new User
                {
                    Username = username,
                    Password = _passwordHasher.HashPassword(Guid.NewGuid().ToString()), // Unguessable password
                    Role = "Employee"
                };
                await _unitOfWork.Users.AddAsync(user);
                changesMade = true;
            }

            // Ensure Employee profile exists
            var employees = await _unitOfWork.Employees.FindAsync(e => e.Email == email);
            if (!employees.Any())
            {
                string displayName = root.TryGetProperty("displayName", out var nameProp) ? nameProp.GetString() : email;

                var employee = new Employee
                {
                    Name = displayName,
                    Email = email,
                    Salary = 0,
                    DepartmentId = null,
                    Projects = new List<Project>()
                };
                await _unitOfWork.Employees.AddAsync(employee);
                changesMade = true;
            }

            if (changesMade)
            {
                await _unitOfWork.CompleteAsync();
            }

            return _jwtTokenGenerator.GenerateToken(user);
        }
    }
}

