using Microsoft.EntityFrameworkCore;
using TaskManagementAPI.DTOs;
using TaskManagementApi.Models;
using TaskManagementAPI.Services;
using TaskManagementAPI.DataAccess;

namespace TaskManagementApi.Services
{
    public class TaskService : ITaskService
    {
        private readonly ApplicationDbContext _context;

        public TaskService(ApplicationDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        /// <summary>
        /// Retrieves all tasks from the database.
        /// </summary>
        /// <returns>A list of all tasks with assigned user data.</returns>
        public async Task<List<TaskItem>> GetAllTasksAsync()
        {
            // Retrieve all tasks including assigned user 
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .ToListAsync();
        }

        /// <summary>
        /// Retrieves a single task by its ID.
        /// </summary>
        /// <param name="id">The ID of the task to retrieve.</param>
        /// <returns>The task with related user and comment data, or null if not found.</returns>
        public async Task<TaskItem> GetTaskByIdAsync(int id)
        {
            // Retrieve a task by ID including assigned user and comments
            return await _context.Tasks
                .Include(t => t.AssignedUser)
                .Include(t => t.Comments)
                    .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Retrieves all tasks assigned to a specific user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <returns>A list of tasks assigned to the specified user.</returns>
        public async Task<List<TaskItem>> GetTasksByUserIdAsync(int userId)
        {
            // Retrieve tasks assigned to a specific user 
            return await _context.Tasks
                .Where(t => t.AssignedToUserId == userId)
                .Include(t => t.AssignedUser)
                .ToListAsync();
        }

        /// <summary>
        /// Creates a new task.
        /// </summary>
        /// <param name="taskDto">The data for the new task.</param>
        /// <returns>The created task entity.</returns>
        public async Task<TaskItem> CreateTaskAsync(TaskItemDto taskDto)
        {
            if (taskDto == null)
            {
                throw new ArgumentNullException(nameof(taskDto));
            }

            // Map DTO to entity 
            var task = new TaskItem
            {
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                IsCompleted = taskDto.IsCompleted,
                AssignedToUserId = taskDto.AssignedToUserId
            };

            // Save task to database 
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Load the related AssignedUser data to match the expected result
            if (task.AssignedToUserId.HasValue)
            {
                await _context.Entry(task)
                    .Reference(t => t.AssignedUser)
                    .LoadAsync();
            }

            return task;
        }

        /// <summary>
        /// Updates an existing task.
        /// </summary>
        /// <param name="id">The ID of the task to update.</param>
        /// <param name="taskDto">The updated task data.</param>
        /// <returns>The updated task, or null if not found.</returns>
        public async Task<TaskItem> UpdateTaskAsync(int id, TaskItemDto taskDto)
        {
            if (taskDto == null)
            {
                throw new ArgumentNullException(nameof(taskDto));
            }

            // Retrieve existing task 
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return null;
            }

            // Update task fields 
            task.Title = taskDto.Title;
            task.Description = taskDto.Description;
            task.DueDate = taskDto.DueDate;
            task.IsCompleted = taskDto.IsCompleted;
            task.AssignedToUserId = taskDto.AssignedToUserId;

            // Save changes 
            await _context.SaveChangesAsync();

            // Load the related AssignedUser data to match the expected result
            if (task.AssignedToUserId.HasValue)
            {
                await _context.Entry(task)
                    .Reference(t => t.AssignedUser)
                    .LoadAsync();
            }

            return task;
        }

        /// <summary>
        /// Deletes a task by its ID.
        /// </summary>
        /// <param name="id">The ID of the task to delete.</param>
        /// <returns>True if deleted successfully; false if not found.</returns>
        public async Task<bool> DeleteTaskAsync(int id)
        {
            // Find task by ID 
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return false;
            }

            // Remove task from database 
            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            return true;
        }

        /// <summary>
        /// Retrieves all comments for a specific task.
        /// </summary>
        /// <param name="taskId">The ID of the task.</param>
        /// <returns>A list of comments with user data.</returns>
        public async Task<List<TaskComment>> GetTaskCommentsAsync(int taskId)
        {
            // Retrieve comments for a specific task 
            return await _context.TaskComments
                .Where(c => c.TaskId == taskId)
                .Include(c => c.User)
                .ToListAsync();
        }

        /// <summary>
        /// Adds a comment to a task.
        /// </summary>
        /// <param name="commentDto">The comment data.</param>
        /// <returns>The created comment with user information.</returns>
        public async Task<TaskComment> AddCommentAsync(TaskCommentDto commentDto)
        {
            if (commentDto == null)
            {
                throw new ArgumentNullException(nameof(commentDto));
            }

            // Map DTO to comment entity 
            var comment = new TaskComment
            {
                Content = commentDto.Content,
                CreatedAt = DateTime.Now,
                TaskId = commentDto.TaskId,
                UserId = commentDto.UserId
            };

            // Save comment to database 
            _context.TaskComments.Add(comment);
            await _context.SaveChangesAsync();

            // Just return the comment without loading related data 
            return comment;
        }
    }
}