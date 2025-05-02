using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace TaskManagementApi.Models
{
    public class TaskComment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public int TaskId { get; set; }

        [Required]
        public int UserId { get; set; }

        [ForeignKey("TaskId")]
        public TaskItem Task { get; set; }

        [ForeignKey("UserId")]
        public  User User { get; set; }
    }
}
