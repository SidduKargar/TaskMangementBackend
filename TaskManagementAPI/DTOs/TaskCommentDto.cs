using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.DTOs
{
    public class TaskCommentDto
    {
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        public DateTime CreatedAt { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }

        public string Username { get; set; }
    }
}
