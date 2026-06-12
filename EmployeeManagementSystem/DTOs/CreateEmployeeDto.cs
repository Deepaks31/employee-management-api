namespace EmployeeManagementSystem.DTOs
{
    public class CreateEmployeeDto
    {
        public string Name { get; set; }

        public decimal Salary { get; set; }

        public string Email { get; set; }

        // Foreign Key
        public int? DepartmentId { get; set; }

        // Many-to-Many Project IDs
        public List<int> ProjectIds { get; set; }
    }
}

