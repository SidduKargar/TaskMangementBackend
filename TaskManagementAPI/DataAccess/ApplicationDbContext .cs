using Microsoft.EntityFrameworkCore;
using TaskManagementApi.Models;

namespace TaskManagementAPI.DataAccess
{
        /// <summary>
        /// The application's database context for interacting with users, tasks, and comments.
        /// </summary>
        public class ApplicationDbContext : DbContext
        {
            /// <summary>
            /// Constructor accepting database options.
            /// </summary>
            /// <param name="options">Database context options.</param>
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

            /// <summary>
            /// Users table in the database.
            /// </summary>
            public DbSet<User> Users { get; set; }

            /// <summary>
            /// Tasks table in the database.
            /// </summary>
            public DbSet<TaskItem> Tasks { get; set; }

            /// <summary>
            /// TaskComments table in the database.
            /// </summary>
            public DbSet<TaskComment> TaskComments { get; set; }

            /// <summary>
            /// Configures entity relationships and seeds initial data.
            /// </summary>
            /// <param name="modelBuilder">Model builder for entity configuration.</param>
            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                //Configure TaskItem-User relationship 
                modelBuilder.Entity<TaskItem>()
                    .HasOne(t => t.AssignedUser)
                    .WithMany(u => u.AssignedTasks)
                    .HasForeignKey(t => t.AssignedToUserId)
                    .OnDelete(DeleteBehavior.SetNull);

                //Configure TaskComment-Task relationship 
                modelBuilder.Entity<TaskComment>()
                    .HasOne(c => c.Task)
                    .WithMany(t => t.Comments)
                    .HasForeignKey(c => c.TaskId)
                    .OnDelete(DeleteBehavior.Cascade);

                //Configure TaskComment-User relationship 
                modelBuilder.Entity<TaskComment>()
                    .HasOne(c => c.User)
                    .WithMany(u => u.Comments)
                    .HasForeignKey(c => c.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                //Seed initial data 
                SeedData(modelBuilder);
            }

            /// <summary>
            /// Seeds initial users, tasks, and comments into the database.
            /// </summary>
            /// <param name="modelBuilder">Model builder used to seed data.</param>
            private void SeedData(ModelBuilder modelBuilder)
            {
                //Seed users 
                modelBuilder.Entity<User>().HasData(
                    new User
                    {
                        Id = 1,
                        Username = "admin",
                        Email = "admin@example.com",
                        Password = "admin123",
                        Role = "Admin"
                    },
                    new User
                    {
                        Id = 2,
                        Username = "user",
                        Email = "user@example.com",
                        Password = "user123",
                        Role = "User"
                    }
                );

                //Seed tasks 
                modelBuilder.Entity<TaskItem>().HasData(
                    new TaskItem
                    {
                        Id = 1,
                        Title = "Complete project documentation",
                        Description = "Write all documentation for the API project",
                        DueDate = DateTime.Now.AddDays(7),
                        IsCompleted = false,
                        AssignedToUserId = 1
                    },
                    new TaskItem
                    {
                        Id = 2,
                        Title = "Implement authentication",
                        Description = "Add JWT authentication to the API",
                        DueDate = DateTime.Now.AddDays(3),
                        IsCompleted = false,
                        AssignedToUserId = 1
                    },
                    new TaskItem
                    {
                        Id = 3,
                        Title = "Test API endpoints",
                        Description = "Create and run tests for all API endpoints",
                        DueDate = DateTime.Now.AddDays(5),
                        IsCompleted = false,
                        AssignedToUserId = 2
                    }
                );

                //Seed comments 
                modelBuilder.Entity<TaskComment>().HasData(
                    new TaskComment
                    {
                        Id = 1,
                        Content = "This needs to be done ASAP",
                        CreatedAt = DateTime.Now.AddDays(-1),
                        TaskId = 1,
                        UserId = 1
                    },
                    new TaskComment
                    {
                        Id = 2,
                        Content = "I'll start working on this tomorrow",
                        CreatedAt = DateTime.Now.AddHours(-12),
                        TaskId = 1,
                        UserId = 2
                    },
                    new TaskComment
                    {
                        Id = 3,
                        Content = "Don't forget to use JWT tokens",
                        CreatedAt = DateTime.Now.AddHours(-6),
                        TaskId = 2,
                        UserId = 1
                    }
                );
            }
        
    }
}
