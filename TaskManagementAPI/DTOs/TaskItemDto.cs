using System.ComponentModel.DataAnnotations;

namespace TaskManagementAPI.DTOs
{
        public class TaskItemDto
        {
            public int Id { get; set; }

            [Required]
            [StringLength(200)]
            public string Title { get; set; }

            public string Description { get; set; }

            [Required]
            public DateTime DueDate { get; set; }

            public bool IsCompleted { get; set; }

            public int? AssignedToUserId { get; set; }
    }
}
