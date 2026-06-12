namespace EmployeeManagementSystem.DTOs
{
    public class EmployeeResponseDto
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Salary { get; set; }

        public string Email { get; set; }

        public string DepartmentName { get; set; }

        public List<string> Projects { get; set; }
    }
}

