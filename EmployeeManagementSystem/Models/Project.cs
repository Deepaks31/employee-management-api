namespace EmployeeManagementSystem.Models
{
    public class Project
    {
        public int Id { get; set; }

        public string ProjectName { get; set; }

        // Many-to-Many
        public List<Employee> Employees { get; set; }
    }
}

