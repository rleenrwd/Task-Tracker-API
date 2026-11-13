using Microsoft.EntityFrameworkCore;
using TaskTracker.Domain.Entities;
using TaskTracker.Infrastructure.Persistence.Configurations;

namespace TaskTracker.Infrastructure.Persistence;


public class TaskTrackerDbContext : DbContext
{
    public DbSet<TaskItem> TaskItems {get;set;}

    public TaskTrackerDbContext(DbContextOptions<TaskTrackerDbContext> options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        new TaskItemEntityTypeConfiguration()
        .Configure(modelBuilder.Entity<TaskItem>());
    }
}
