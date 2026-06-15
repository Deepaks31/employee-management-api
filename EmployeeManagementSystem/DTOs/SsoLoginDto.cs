using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.DTOs
{
    public class SsoLoginDto
    {
        [Required]
        public string Token { get; set; }
    }
}
