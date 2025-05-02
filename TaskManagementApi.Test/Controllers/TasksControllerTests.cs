using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TaskManagementApi.Controllers;
using TaskManagementApi.Models;
using TaskManagementAPI.DTOs;
using TaskManagementAPI.Services;
using TaskManagementAPI.Tests.Mocks;

namespace TaskManagementAPI.Tests.Controllers
{
    public class TasksControllerTests
    {
        private readonly Mock<ITaskService> _mockTaskService;
        private readonly TasksController _controller;

        //Setup for admin user
        private readonly ClaimsPrincipal _adminUser;

        // Setup for regular user
        private readonly ClaimsPrincipal _regularUser;

        // Setup for another regular user (for testing access control)
        private readonly ClaimsPrincipal _anotherUser;

        public TasksControllerTests()
        {
            _mockTaskService = new Mock<ITaskService>();
            _controller = new TasksController(_mockTaskService.Object);

            // Create admin user claims
            _adminUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "mock"));

            // Create regular user claims (ID: 1)
            _regularUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            // Create another user claims (ID: 2)
            _anotherUser = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, "2")
            }, "mock"));
        }

        #region GetAllTasks

        [Fact]
        public async Task GetAllTasks_ShouldReturnAllTasks_ForAdminUser()
        {
            // Arrange
            var mockTasks = MockData.GetMockTasks();
            _mockTaskService.Setup(service => service.GetAllTasksAsync())
                .ReturnsAsync(mockTasks);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _adminUser }
            };

            // Act
            var result = await _controller.GetAllTasks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<TaskItem>>(okResult.Value);
            Assert.Equal(mockTasks.Count, returnValue.Count);
            _mockTaskService.Verify(service => service.GetAllTasksAsync(), Times.Once);
            _mockTaskService.Verify(service => service.GetTasksByUserIdAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetAllTasks_ShouldReturnUserTasks_ForRegularUser()
        {
            // Arrange
            var userTasks = MockData.GetMockTasks().FindAll(t => t.AssignedToUserId == 1);
            _mockTaskService.Setup(service => service.GetTasksByUserIdAsync(1))
                .ReturnsAsync(userTasks);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _regularUser }
            };

            // Act
            var result = await _controller.GetAllTasks();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<TaskItem>>(okResult.Value);
            Assert.Equal(userTasks.Count, returnValue.Count);
            _mockTaskService.Verify(service => service.GetAllTasksAsync(), Times.Never);
            _mockTaskService.Verify(service => service.GetTasksByUserIdAsync(1), Times.Once);
        }

        [Fact]
        public async Task GetAllTasks_ShouldReturnUnauthorized_WhenUserIdentifierIsMissing()
        {
            // Arrange
            var userWithoutId = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] { }, "mock"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userWithoutId }
            };

            // Act
            var result = await _controller.GetAllTasks();

            // Assert
            Assert.IsType<UnauthorizedObjectResult>(result.Result);
        }

        #endregion

        #region GetTask

        [Fact]
        public async Task GetTask_ShouldReturnTask_WhenAdminRequests()
        {
            // Arrange
            var mockTask = MockData.GetSingleTask();
            _mockTaskService.Setup(service => service.GetTaskByIdAsync(mockTask.Id))
                .ReturnsAsync(mockTask);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _adminUser }
            };

            // Act
            var result = await _controller.GetTask(mockTask.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<TaskItem>(okResult.Value);
            Assert.Equal(mockTask.Id, returnValue.Id);
        }

        [Fact]
        public async Task GetTask_ShouldReturnTask_WhenAssignedUserRequests()
        {
            // Arrange
            var mockTask = MockData.GetSingleTask(); // Task assigned to user ID 1
            _mockTaskService.Setup(service => service.GetTaskByIdAsync(mockTask.Id))
                .ReturnsAsync(mockTask);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _regularUser } // User ID 1
            };

            // Act
            var result = await _controller.GetTask(mockTask.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<TaskItem>(okResult.Value);
            Assert.Equal(mockTask.Id, returnValue.Id);
        }

        [Fact]
        public async Task GetTask_ShouldReturnForbid_WhenUnassignedUserRequests()
        {
            // Arrange
            var mockTask = MockData.GetSingleTask(); // Task assigned to user ID 1
            _mockTaskService.Setup(service => service.GetTaskByIdAsync(mockTask.Id))
                .ReturnsAsync(mockTask);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _anotherUser } // User ID 2
            };

            // Act
            var result = await _controller.GetTask(mockTask.Id);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
        }

        [Fact]
        public async Task GetTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            _mockTaskService.Setup(service => service.GetTaskByIdAsync(999))
                .ReturnsAsync((TaskItem)null);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _adminUser }
            };

            // Act
            var result = await _controller.GetTask(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        #endregion

        #region GetTasksByUser

        [Fact]
        public async Task GetTasksByUser_ShouldReturnTasks_WhenAdminRequests()
        {
            // Arrange
            var userTasks = MockData.GetMockTasks().FindAll(t => t.AssignedToUserId == 1);
            _mockTaskService.Setup(service => service.GetTasksByUserIdAsync(1))
                .ReturnsAsync(userTasks);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _adminUser }
            };

            // Act
            var result = await _controller.GetTasksByUser(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<TaskItem>>(okResult.Value);
            Assert.Equal(userTasks.Count, returnValue.Count);
        }

        [Fact]
        public async Task GetTasksByUser_ShouldReturnTasks_WhenOwnUserRequests()
        {
            // Arrange
            var userTasks = MockData.GetMockTasks().FindAll(t => t.AssignedToUserId == 1);
            _mockTaskService.Setup(service => service.GetTasksByUserIdAsync(1))
                .ReturnsAsync(userTasks);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _regularUser } // User ID 1
            };

            // Act
            var result = await _controller.GetTasksByUser(1); // Request user ID 1

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<TaskItem>>(okResult.Value);
            Assert.Equal(userTasks.Count, returnValue.Count);
        }

        [Fact]
        public async Task GetTasksByUser_ShouldReturnForbid_WhenOtherUserRequests()
        {
            // Arrange
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _regularUser } // User ID 1
            };

            // Act
            var result = await _controller.GetTasksByUser(2); // Request user ID 2

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
            _mockTaskService.Verify(service => service.GetTasksByUserIdAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region CreateTask

        [Fact]
        public async Task CreateTask_ShouldCreateTask_WhenAdminAssignsToOthers()
        {
            // Arrange
            var taskDto = MockData.GetValidTaskDto();
            taskDto.AssignedToUserId = 2; // Assign to user 2

            var createdTask = new TaskItem
            {
                Id = 4,
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                IsCompleted = taskDto.IsCompleted,
                AssignedToUserId = taskDto.AssignedToUserId
            };

            _mockTaskService.Setup(service => service.CreateTaskAsync(It.IsAny<TaskItemDto>()))
                .ReturnsAsync(createdTask);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _adminUser }
            };

            // Act
            var result = await _controller.CreateTask(taskDto);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<TaskItem>(createdAtResult.Value);
            Assert.Equal(createdTask.Id, returnValue.Id);
            Assert.Equal(taskDto.Title, returnValue.Title);
            _mockTaskService.Verify(service => service.CreateTaskAsync(It.IsAny<TaskItemDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateTask_ShouldCreateTask_WhenUserAssignsToSelf()
        {
            // Arrange
            var taskDto = MockData.GetValidTaskDto();
            taskDto.AssignedToUserId = 1; // Assign to self (user 1)

            var createdTask = new TaskItem
            {
                Id = 4,
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                IsCompleted = taskDto.IsCompleted,
                AssignedToUserId = taskDto.AssignedToUserId
            };

            _mockTaskService.Setup(service => service.CreateTaskAsync(It.IsAny<TaskItemDto>()))
                .ReturnsAsync(createdTask);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _regularUser } // User ID 1
            };

            // Act
            var result = await _controller.CreateTask(taskDto);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            var returnValue = Assert.IsType<TaskItem>(createdAtResult.Value);
            Assert.Equal(createdTask.Id, returnValue.Id);
            _mockTaskService.Verify(service => service.CreateTaskAsync(It.IsAny<TaskItemDto>()), Times.Once);
        }

        [Fact]
        public async Task CreateTask_ShouldReturnForbid_WhenUserAssignsToOthers()
        {
            // Arrange
            var taskDto = MockData.GetValidTaskDto();
            taskDto.AssignedToUserId = 2; // Assign to user 2

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _regularUser } // User ID 1
            };

            // Act
            var result = await _controller.CreateTask(taskDto);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
            _mockTaskService.Verify(service => service.CreateTaskAsync(It.IsAny<TaskItemDto>()), Times.Never);
        }

        #endregion

        #region UpdateTask

        [Fact]
        public async Task UpdateTask_ShouldUpdateTask_WhenAdminUpdates()
        {
            // Arrange
            var existingTask = MockData.GetSingleTask(); // Task assigned to user 1
            var taskDto = MockData.GetValidTaskDto();
            taskDto.Title = "Updated by Admin";
            taskDto.AssignedToUserId = 2; // Reassign to user 2

            var updatedTask = new TaskItem
            {
                Id = existingTask.Id,
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                IsCompleted = taskDto.IsCompleted,
                AssignedToUserId = taskDto.AssignedToUserId
            };

            _mockTaskService.Setup(service => service.GetTaskByIdAsync(existingTask.Id))
                .ReturnsAsync(existingTask);
            _mockTaskService.Setup(service => service.UpdateTaskAsync(existingTask.Id, It.IsAny<TaskItemDto>()))
                .ReturnsAsync(updatedTask);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _adminUser }
            };

            // Act
            var result = await _controller.UpdateTask(existingTask.Id, taskDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<TaskItem>(okResult.Value);
            Assert.Equal(updatedTask.Id, returnValue.Id);
            Assert.Equal(taskDto.Title, returnValue.Title);
            _mockTaskService.Verify(service => service.UpdateTaskAsync(existingTask.Id, It.IsAny<TaskItemDto>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTask_ShouldUpdateTask_WhenAssignedUserUpdates()
        {
            // Arrange
            var existingTask = MockData.GetSingleTask(); // Task assigned to user 1
            var taskDto = MockData.GetValidTaskDto();
            taskDto.Title = "Updated by Assigned User";

            var updatedTask = new TaskItem
            {
                Id = existingTask.Id,
                Title = taskDto.Title,
                Description = taskDto.Description,
                DueDate = taskDto.DueDate,
                IsCompleted = taskDto.IsCompleted,
                AssignedToUserId = existingTask.AssignedToUserId // Keep same assignment
            };

            _mockTaskService.Setup(service => service.GetTaskByIdAsync(existingTask.Id))
                .ReturnsAsync(existingTask);
            _mockTaskService.Setup(service => service.UpdateTaskAsync(existingTask.Id, It.IsAny<TaskItemDto>()))
                .ReturnsAsync(updatedTask);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _regularUser } // User ID 1
            };

            // Act
            var result = await _controller.UpdateTask(existingTask.Id, taskDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            var returnValue = Assert.IsType<TaskItem>(okResult.Value);
            Assert.Equal(updatedTask.Id, returnValue.Id);
            Assert.Equal(taskDto.Title, returnValue.Title);
            _mockTaskService.Verify(service => service.UpdateTaskAsync(existingTask.Id, It.IsAny<TaskItemDto>()), Times.Once);
        }

        [Fact]
        public async Task UpdateTask_ShouldReturnForbid_WhenUnassignedUserUpdates()
        {
            // Arrange
            var existingTask = MockData.GetSingleTask(); // Task assigned to user 1
            var taskDto = MockData.GetValidTaskDto();

            _mockTaskService.Setup(service => service.GetTaskByIdAsync(existingTask.Id))
                .ReturnsAsync(existingTask);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _anotherUser } // User ID 2
            };

            // Act
            var result = await _controller.UpdateTask(existingTask.Id, taskDto);

            // Assert
            Assert.IsType<ForbidResult>(result);
            _mockTaskService.Verify(service => service.UpdateTaskAsync(It.IsAny<int>(), It.IsAny<TaskItemDto>()), Times.Never);
        }

        [Fact]
        public async Task UpdateTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var taskDto = MockData.GetValidTaskDto();

            _mockTaskService.Setup(service => service.GetTaskByIdAsync(999))
                .ReturnsAsync((TaskItem)null);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _adminUser }
            };

            // Act
            var result = await _controller.UpdateTask(999, taskDto);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockTaskService.Verify(service => service.UpdateTaskAsync(It.IsAny<int>(), It.IsAny<TaskItemDto>()), Times.Never);
        }

        #endregion

        #region DeleteTask

        [Fact]
        public async Task DeleteTask_ShouldDeleteTask_WhenAdminDeletes()
        {
            // Arrange
            _mockTaskService.Setup(service => service.DeleteTaskAsync(1))
                .ReturnsAsync(true);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _adminUser }
            };

            // Act
            var result = await _controller.DeleteTask(1);

            // Assert
            Assert.IsType<NoContentResult>(result);
            _mockTaskService.Verify(service => service.DeleteTaskAsync(1), Times.Once);
        }

        [Fact]
        public async Task DeleteTask_ShouldReturnNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            _mockTaskService.Setup(service => service.DeleteTaskAsync(999))
                .ReturnsAsync(false);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _adminUser }
            };

            // Act
            var result = await _controller.DeleteTask(999);

            // Assert
            Assert.IsType<NotFoundResult>(result);
            _mockTaskService.Verify(service => service.DeleteTaskAsync(999), Times.Once);
        }

        #endregion

        #region GetTaskComments

        [Fact]
        public async Task GetTaskComments_ShouldReturnComments_WhenAdminRequests()
        {
            // Arrange
            var mockTask = MockData.GetSingleTask();
            var mockComments = MockData.GetMockComments().FindAll(c => c.TaskId == mockTask.Id);

            _mockTaskService.Setup(service => service.GetTaskByIdAsync(mockTask.Id))
                .ReturnsAsync(mockTask);
            _mockTaskService.Setup(service => service.GetTaskCommentsAsync(mockTask.Id))
                .ReturnsAsync(mockComments);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _adminUser }
            };

            // Act
            var result = await _controller.GetTaskComments(mockTask.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<TaskComment>>(okResult.Value);
            Assert.Equal(mockComments.Count, returnValue.Count);
        }

        [Fact]
        public async Task GetTaskComments_ShouldReturnComments_WhenAssignedUserRequests()
        {
            // Arrange
            var mockTask = MockData.GetSingleTask(); // Task assigned to user 1
            var mockComments = MockData.GetMockComments().FindAll(c => c.TaskId == mockTask.Id);

            _mockTaskService.Setup(service => service.GetTaskByIdAsync(mockTask.Id))
                .ReturnsAsync(mockTask);
            _mockTaskService.Setup(service => service.GetTaskCommentsAsync(mockTask.Id))
                .ReturnsAsync(mockComments);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _regularUser } // User ID 1
            };

            // Act
            var result = await _controller.GetTaskComments(mockTask.Id);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<List<TaskComment>>(okResult.Value);
            Assert.Equal(mockComments.Count, returnValue.Count);
        }

        [Fact]
        public async Task GetTaskComments_ShouldReturnForbid_WhenUnassignedUserRequests()
        {
            // Arrange
            var mockTask = MockData.GetSingleTask(); // Task assigned to user 1

            _mockTaskService.Setup(service => service.GetTaskByIdAsync(mockTask.Id))
                .ReturnsAsync(mockTask);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _anotherUser } // User ID 2
            };

            // Act
            var result = await _controller.GetTaskComments(mockTask.Id);

            // Assert
            Assert.IsType<ForbidResult>(result.Result);
            _mockTaskService.Verify(service => service.GetTaskCommentsAsync(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetTaskComments_ShouldReturnNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            _mockTaskService.Setup(service => service.GetTaskByIdAsync(999))
                .ReturnsAsync((TaskItem)null);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _adminUser }
            };

            // Act
            var result = await _controller.GetTaskComments(999);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
            _mockTaskService.Verify(service => service.GetTaskCommentsAsync(It.IsAny<int>()), Times.Never);
        }

        #endregion

        #region AddComment

        [Fact]
        public async Task AddComment_ShouldAddComment_WhenTaskExists()
        {
            // Arrange
            var mockTask = MockData.GetSingleTask();
            var commentDto = MockData.GetValidCommentDto();
            var createdComment = new TaskComment
            {
                Id = 4,
                Content = commentDto.Content,
                CreatedAt = System.DateTime.Now,
                TaskId = mockTask.Id,
                UserId = 1,
                User = new User { Id = 1, Username = "John Doe" }
            };

            _mockTaskService.Setup(service => service.GetTaskByIdAsync(mockTask.Id))
                .ReturnsAsync(mockTask);
            _mockTaskService.Setup(service => service.AddCommentAsync(It.IsAny<TaskCommentDto>()))
                .ReturnsAsync(createdComment);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _regularUser } // User ID 1
            };

            // Act
            var result = await _controller.AddComment(mockTask.Id, commentDto);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var returnValue = Assert.IsType<TaskComment>(okResult.Value);
            Assert.Equal(createdComment.Id, returnValue.Id);
            Assert.Equal(commentDto.Content, returnValue.Content);
            Assert.Equal(1, returnValue.UserId); // Comment should be associated with current user (ID 1)
            Assert.Equal(mockTask.Id, returnValue.TaskId);
            _mockTaskService.Verify(service => service.AddCommentAsync(It.IsAny<TaskCommentDto>()), Times.Once);
        }

        [Fact]
        public async Task AddComment_ShouldReturnNotFound_WhenTaskDoesNotExist()
        {
            // Arrange
            var commentDto = MockData.GetValidCommentDto();

            _mockTaskService.Setup(service => service.GetTaskByIdAsync(999))
                .ReturnsAsync((TaskItem)null);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = _regularUser }
            };

            // Act
            var result = await _controller.AddComment(999, commentDto);

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
            _mockTaskService.Verify(service => service.AddCommentAsync(It.IsAny<TaskCommentDto>()), Times.Never);
        }

        #endregion
    }
}