using EmployeeManagement.API.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeManagement.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<Department> Departments { get; set; } = null!;
        public DbSet<User> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Day 9: Fluent API relationship configuration
            modelBuilder.Entity<Employee>()
                .HasOne(e => e.DepartmentEntity)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.SetNull);

            // Seed initial departments
            modelBuilder.Entity<Department>().HasData(
                new Department { Id = 1, Name = "Engineering", Description = "Software and Platform Engineering" },
                new Department { Id = 2, Name = "Human Resources", Description = "People Operations and Recruitment" },
                new Department { Id = 3, Name = "Finance", Description = "Financial Planning and Accounting" }
            );
        }
    }
}
