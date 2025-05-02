using TaskManagementAPI.DTOs;
using TaskManagementApi.Models;

namespace TaskManagementAPI.Services
{
    public interface ITaskService
    {
        Task<List<TaskItem>> GetAllTasksAsync();
        Task<TaskItem> GetTaskByIdAsync(int id);
        Task<List<TaskItem>> GetTasksByUserIdAsync(int userId);
        Task<TaskItem> CreateTaskAsync(TaskItemDto taskDto);
        Task<TaskItem> UpdateTaskAsync(int id, TaskItemDto taskDto);
        Task<bool> DeleteTaskAsync(int id);
        Task<List<TaskComment>> GetTaskCommentsAsync(int taskId);
        Task<TaskComment> AddCommentAsync(TaskCommentDto commentDto);
    }
}
