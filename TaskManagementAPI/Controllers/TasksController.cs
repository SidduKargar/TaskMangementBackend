using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskManagementAPI.DTOs;
using TaskManagementApi.Models;
using TaskManagementAPI.Services;

namespace TaskManagementApi.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly ITaskService _taskService;

        public TasksController(ITaskService taskService)
        {
            _taskService = taskService;
        }

        /// <summary>
        /// Returns all tasks for admin users, or tasks assigned to the authenticated user.
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<List<TaskItem>>> GetAllTasks()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null)
                return Unauthorized("Missing NameIdentifier claim in JWT token.");

            if (!User.IsInRole("Admin"))
            {
                // Regular users only see their own tasks
                var userId = int.Parse(userIdClaim.Value);
                var userTasks = await _taskService.GetTasksByUserIdAsync(userId);
                return Ok(userTasks);
            }

            // Admins see all tasks
            var tasks = await _taskService.GetAllTasksAsync();
            return Ok(tasks);
        }

        /// <summary>
        /// Returns a specific task by ID, with access control based on role or ownership.
        /// </summary>
        [HttpGet]
        [Route("{id}")]
        public async Task<ActionResult<TaskItem>> GetTask(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            // Only the assigned user or admin can access this task
            if (!User.IsInRole("Admin") &&
                task.AssignedToUserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid("User does not have the required roles.");
            }

            return Ok(task);
        }

        /// <summary>
        /// Gets tasks assigned to a specific user. Only accessible by Admins or the user themselves.
        /// </summary>
        [HttpGet]
        [Route("user/{userId}")]
        public async Task<ActionResult<IEnumerable<TaskItem>>> GetTasksByUser(int userId)
        {
            // Ensure requester is either Admin or the user themselves
            if (!User.IsInRole("Admin") &&
                userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid("User does not have the required roles.");
            }

            var tasks = await _taskService.GetTasksByUserIdAsync(userId);
            return Ok(tasks);
        }

        /// <summary>
        /// Creates a new task. Admins can assign to anyone, users can only assign to themselves.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<TaskItem>> CreateTask(TaskItemDto taskDto)
        {
            // Only Admins can assign tasks to others
            if (!User.IsInRole("Admin") &&
                taskDto.AssignedToUserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid("User does not have the required roles.");
            }

            var task = await _taskService.CreateTaskAsync(taskDto);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, task);
        }

        /// <summary>
        /// Updates an existing task. Only Admins or the assigned user can perform the update.
        /// </summary>
        [HttpPut]
        public async Task<IActionResult> UpdateTask(int id, TaskItemDto taskDto)
        {
            var existingTask = await _taskService.GetTaskByIdAsync(id);

            if (existingTask == null)
            {
                return NotFound();
            }

            // Ensure the requester has access to update the task
            if (!User.IsInRole("Admin") &&
                existingTask.AssignedToUserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid("User does not have the required roles.");
            }

            var task = await _taskService.UpdateTaskAsync(id, taskDto);
            return Ok(task);
        }

        /// <summary>
        /// Deletes a task by ID. Only Admins are authorized to delete tasks.
        /// </summary>
        [HttpDelete]
        [Route("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTask(int id)
        {
            var result = await _taskService.DeleteTaskAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }

        /// <summary>
        /// Gets all comments for a specific task. Only Admins or the assigned user can access.
        /// </summary>
        [HttpGet]
        [Route("{id}/comments")]
        public async Task<ActionResult<IEnumerable<TaskComment>>> GetTaskComments(int id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            // Ensure the requester can access task comments
            if (!User.IsInRole("Admin") &&
                task.AssignedToUserId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Forbid("User does not have the required roles.");
            }

            var comments = await _taskService.GetTaskCommentsAsync(id);
            return Ok(comments);
        }

        /// <summary>
        /// Adds a comment to a task. User must be authenticated. Comment is attributed to the requester.
        /// </summary>
        [HttpPost]
        [Route("{id}/comments")]
        public async Task<ActionResult<TaskComment>> AddComment(int id, TaskCommentDto commentDto)
        {
            var task = await _taskService.GetTaskByIdAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            // Automatically associate comment with current user and task
            commentDto.UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            commentDto.TaskId = id;

            var comment = await _taskService.AddCommentAsync(commentDto);

            return Ok(comment);
        }
    }
}
