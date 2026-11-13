using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskTracker.Domain.Entities;

namespace TaskTracker.Infrastructure.Persistence.Configurations;

public class TaskItemEntityTypeConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder
        .ToTable("TaskItems")
        .HasKey(taskItem => taskItem.Id);

        builder
        .Property(taskItem => taskItem.Title)
        .IsRequired();

        builder
        .Property(taskItem => taskItem.Description)
        .IsRequired(false);

        builder
        .Property(taskItem => taskItem.Status)
        .HasConversion<string>()
        .HasMaxLength(32)
        .IsRequired();

        builder
        .Property(taskItem => taskItem.CreatedAt)
        .IsRequired();

        builder
        .Property(taskItem => taskItem.SubmittedAt)
        .IsRequired(false);

    }
}