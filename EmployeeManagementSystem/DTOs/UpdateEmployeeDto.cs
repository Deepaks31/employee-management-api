namespace EmployeeManagementSystem.DTOs
{
    public class UpdateEmployeeDto
    {
        public string Name { get; set; }

        public decimal Salary { get; set; }

        public string Email { get; set; }

        public int? DepartmentId { get; set; }

        public List<int> ProjectIds { get; set; }
    }
}

