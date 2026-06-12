namespace EmployeeManagementSystem.Models
{
    public class Employee
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public decimal Salary { get; set; }

        public string Email { get; set; }

        // Foreign Key
        public int? DepartmentId { get; set; }

        // Navigation Property
        public Department Department { get; set; }

        // Many-to-Many
        public List<Project> Projects { get; set; }
    }
}

