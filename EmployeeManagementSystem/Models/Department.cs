namespace EmployeeManagementSystem.Models
{
    public class Department
    {
        public int Id { get; set; }

        public string Name { get; set; }

        // Navigation Property
        public List<Employee> Employees { get; set; }
    }
}

