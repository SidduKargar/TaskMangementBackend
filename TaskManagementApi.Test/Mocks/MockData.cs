using System;
using System.Collections.Generic;
using TaskManagementApi.Models;
using TaskManagementAPI.DTOs;

namespace TaskManagementAPI.Tests.Mocks
{
    public static class MockData
    {
        public static List<TaskItem> GetMockTasks() => new List<TaskItem>
        {
            new TaskItem
            {
                Id = 1,
                Title = "Task 1",
                Description = "Description 1",
                DueDate = DateTime.Now.AddDays(1),
                IsCompleted = false,
                AssignedToUserId = 1
            },
            new TaskItem
            {
                Id = 2,
                Title = "Task 2",
                Description = "Description 2",
                DueDate = DateTime.Now.AddDays(2),
                IsCompleted = false,
                AssignedToUserId = 1
            },
            new TaskItem
            {
                Id = 3,
                Title = "Task 3",
                Description = "Description 3",
                DueDate = DateTime.Now.AddDays(3),
                IsCompleted = true,
                AssignedToUserId = 2
            }
        };

        public static TaskItem GetSingleTask() => new TaskItem
        {
            Id = 1,
            Title = "Sample Task",
            Description = "Sample Description",
            DueDate = DateTime.Now.AddDays(5),
            IsCompleted = false,
            AssignedToUserId = 1
        };

        public static List<TaskComment> GetMockComments() => new List<TaskComment>
        {
            new TaskComment
            {
                Id = 1,
                TaskId = 1,
                UserId = 1,
                Content = "This is a comment for task 1",
                CreatedAt = DateTime.Now
            },
            new TaskComment
            {
                Id = 2,
                TaskId = 3,
                UserId = 2,
                Content = "Another comment for a different task",
                CreatedAt = DateTime.Now
            }
        };

        public static TaskItemDto GetValidTaskDto() => new TaskItemDto
        {
            Title = "New Task",
            Description = "New Task Description",
            DueDate = DateTime.Now.AddDays(7),
            IsCompleted = false,
            AssignedToUserId = 1
        };

        public static TaskCommentDto GetValidCommentDto() => new TaskCommentDto
        {
            TaskId = 1,
            UserId = 1,
            Content = "This is a new comment"
        };
    }
}
