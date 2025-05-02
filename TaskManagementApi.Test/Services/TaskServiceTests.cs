using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Models;
using TaskManagementApi.Services;
using TaskManagementAPI.DataAccess;
using TaskManagementAPI.Tests.Mocks;

namespace TaskManagementAPI.Tests.Services
{
    public class TaskServiceTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public TaskServiceTests()
        {
            // Create a fresh in-memory database for each test
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .EnableSensitiveDataLogging()
                .Options;
        }

        #region GetAllTasksAsync

        [Fact]
        public async Task GetAllTasksAsync_ShouldReturnAllTasks()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var mockTasks = MockData.GetMockTasks();
            await context.Tasks.AddRangeAsync(mockTasks);
            await context.SaveChangesAsync();

            var service = new TaskService(context);

            // Act
            var result = await service.GetAllTasksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockTasks.Count, result.Count);
            Assert.Equal(mockTasks[0].Title, result[0].Title);
            Assert.Equal(mockTasks[1].Title, result[1].Title);
            Assert.Equal(mockTasks[2].Title, result[2].Title);
        }

        [Fact]
        public async Task GetAllTasksAsync_ShouldReturnEmptyList_WhenNoTasksExist()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var service = new TaskService(context);

            // Act
            var result = await service.GetAllTasksAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region GetTaskByIdAsync

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnTask_WhenTaskExists()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var mockTask = MockData.GetSingleTask();
            await context.Tasks.AddAsync(mockTask);
            await context.SaveChangesAsync();

            var service = new TaskService(context);

            // Act
            var result = await service.GetTaskByIdAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockTask.Id, result.Id);
            Assert.Equal(mockTask.Title, result.Title);
            Assert.Equal(mockTask.Description, result.Description);
        }

        [Fact]
        public async Task GetTaskByIdAsync_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var service = new TaskService(context);

            // Act
            var result = await service.GetTaskByIdAsync(999); // Non-existent ID

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region GetTasksByUserIdAsync

        [Fact]
        public async Task GetTasksByUserIdAsync_ShouldReturnUserTasks_WhenUserHasTasks()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var mockTasks = MockData.GetMockTasks();
            await context.Tasks.AddRangeAsync(mockTasks);
            await context.SaveChangesAsync();

            var service = new TaskService(context);
            var userId = 1; // User ID with tasks

            // Act
            var result = await service.GetTasksByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count); // User with ID 1 has 2 tasks in mock data
            Assert.All(result, task => Assert.Equal(userId, task.AssignedToUserId));
        }

        [Fact]
        public async Task GetTasksByUserIdAsync_ShouldReturnEmptyList_WhenUserHasNoTasks()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var mockTasks = MockData.GetMockTasks();
            await context.Tasks.AddRangeAsync(mockTasks);
            await context.SaveChangesAsync();

            var service = new TaskService(context);
            var userId = 999; // User ID with no tasks

            // Act
            var result = await service.GetTasksByUserIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region CreateTaskAsync

        [Fact]
        public async Task CreateTaskAsync_ShouldCreateAndReturnTask_WithValidData()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var service = new TaskService(context);
            var taskDto = MockData.GetValidTaskDto();

            // Act
            var result = await service.CreateTaskAsync(taskDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(taskDto.Title, result.Title);
            Assert.Equal(taskDto.Description, result.Description);
            Assert.Equal(taskDto.DueDate, result.DueDate);
            Assert.Equal(taskDto.IsCompleted, result.IsCompleted);
            Assert.Equal(taskDto.AssignedToUserId, result.AssignedToUserId);

            // Verify task was added to database
            var savedTask = await context.Tasks.FirstOrDefaultAsync(t => t.Id == result.Id);
            Assert.NotNull(savedTask);
            Assert.Equal(taskDto.Title, savedTask.Title);
        }

        #endregion

        #region UpdateTaskAsync

        [Fact]
        public async Task UpdateTaskAsync_ShouldUpdateAndReturnTask_WhenTaskExists()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var mockTask = MockData.GetSingleTask();
            await context.Tasks.AddAsync(mockTask);
            await context.SaveChangesAsync();

            var service = new TaskService(context);
            var taskDto = MockData.GetValidTaskDto();
            taskDto.Title = "Updated Title";
            taskDto.Description = "Updated Description";

            // Act
            var result = await service.UpdateTaskAsync(mockTask.Id, taskDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(mockTask.Id, result.Id);
            Assert.Equal(taskDto.Title, result.Title);
            Assert.Equal(taskDto.Description, result.Description);

            // Verify changes were saved to database
            await using var verifyContext = new ApplicationDbContext(_options);
            var updatedTask = await verifyContext.Tasks.FindAsync(mockTask.Id);
            Assert.NotNull(updatedTask);
            Assert.Equal(taskDto.Title, updatedTask.Title);
            Assert.Equal(taskDto.Description, updatedTask.Description);
        }

        [Fact]
        public async Task UpdateTaskAsync_ShouldReturnNull_WhenTaskDoesNotExist()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var service = new TaskService(context);
            var taskDto = MockData.GetValidTaskDto();

            // Act
            var result = await service.UpdateTaskAsync(999, taskDto); // Non-existent ID

            // Assert
            Assert.Null(result);
        }

        #endregion

        #region DeleteTaskAsync

        [Fact]
        public async Task DeleteTaskAsync_ShouldReturnTrue_WhenTaskExistsAndIsDeleted()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var mockTask = MockData.GetSingleTask();
            await context.Tasks.AddAsync(mockTask);
            await context.SaveChangesAsync();

            var service = new TaskService(context);

            // Act
            var result = await service.DeleteTaskAsync(mockTask.Id);

            // Assert
            Assert.True(result);

            // Verify task was removed from database
            await using var verifyContext = new ApplicationDbContext(_options);
            var deletedTask = await verifyContext.Tasks.FindAsync(mockTask.Id);
            Assert.Null(deletedTask);
        }

        [Fact]
        public async Task DeleteTaskAsync_ShouldReturnFalse_WhenTaskDoesNotExist()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var service = new TaskService(context);

            // Act
            var result = await service.DeleteTaskAsync(999); // Non-existent ID

            // Assert
            Assert.False(result);
        }

        #endregion

        #region GetTaskCommentsAsync

        [Fact]
        public async Task GetTaskCommentsAsync_ShouldReturnComments_WhenTaskHasComments()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);

            // Ensure user exists for the comments
            var user = new User
            {
                Id = 1,
                Username = "John Doe",
                Email = "johndoe@example.com",  
                Password = "Password123",       
                Role = "User"                
            };
            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            // Create and add task
            var mockTask = MockData.GetSingleTask();  // mockTask.Id should be 1
            await context.Tasks.AddAsync(mockTask);

            // Create comments with TaskId that matches the mockTask.Id
            var mockComments = MockData.GetMockComments()
                .Where(c => c.TaskId == mockTask.Id)  // Ensure comments refer to the correct TaskId
                .ToList();

            // Ensure TaskId and UserId are valid in the comment
            mockComments.ForEach(c => c.UserId = user.Id);

            // Add comments to the database
            await context.TaskComments.AddRangeAsync(mockComments);
            await context.SaveChangesAsync();

            // Check if comments are added to the database
            var commentsInDb = await context.TaskComments
                .Where(c => c.TaskId == mockTask.Id)
                .ToListAsync();

            Assert.NotEmpty(commentsInDb); // Ensure comments were saved

            var service = new TaskService(context);

            // Act
            var result = await service.GetTaskCommentsAsync(mockTask.Id);

            // Assert
            Assert.NotNull(result); // Ensure result is not null
            Assert.Single(result); // Ensure only 1 comment is returned
            Assert.Equal(mockComments[0].Content, result[0].Content); // Assert the content of the comment
        }


        [Fact]
        public async Task GetTaskCommentsAsync_ShouldReturnEmptyList_WhenTaskHasNoComments()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var mockTask = new TaskItem
            {
                Id = 4,
                Title = "Task with no comments",
                Description = "This task has no comments",
                DueDate = DateTime.Now.AddDays(5),
                IsCompleted = false,
                AssignedToUserId = 1
            };

            await context.Tasks.AddAsync(mockTask);
            await context.SaveChangesAsync();

            var service = new TaskService(context);

            // Act
            var result = await service.GetTaskCommentsAsync(mockTask.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        #endregion

        #region AddCommentAsync

        [Fact]
        public async Task AddCommentAsync_ShouldAddAndReturnComment_WithValidData()
        {
            // Arrange
            await using var context = new ApplicationDbContext(_options);
            var mockTask = MockData.GetSingleTask();

            await context.Tasks.AddAsync(mockTask);
            await context.SaveChangesAsync();

            // Ensure only one user instance with Id = 1 is added
            var existingUser = await context.Users.FindAsync(1);
            if (existingUser == null)
            {
                var mockUser = new User {
                    Id = 1,
                    Username = "John Doe",
                    Email = "johndoe@example.com",
                    Password = "Password123",
                    Role = "User"
                };
                context.Users.Add(mockUser);
                await context.SaveChangesAsync();
            }

            var service = new TaskService(context);
            var commentDto = MockData.GetValidCommentDto();

            // Act
            var result = await service.AddCommentAsync(commentDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(commentDto.Content, result.Content);
            Assert.Equal(commentDto.TaskId, result.TaskId);
            Assert.Equal(commentDto.UserId, result.UserId);
            Assert.NotEqual(DateTime.MinValue, result.CreatedAt);

            // Verify comment was added to database
            await using var verifyContext = new ApplicationDbContext(_options);
            var savedComment = await verifyContext.TaskComments.FirstOrDefaultAsync(c => c.Content == commentDto.Content);
            Assert.NotNull(savedComment);
        }

        #endregion
    }
}
