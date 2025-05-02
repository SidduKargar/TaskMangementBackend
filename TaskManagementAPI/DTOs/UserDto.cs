using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Role { get; set; }
    }
}
