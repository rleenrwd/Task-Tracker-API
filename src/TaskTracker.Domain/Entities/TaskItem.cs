using System.Data.Common;
using TaskTracker.Domain.Enums;

namespace TaskTracker.Domain.Entities;

public sealed class TaskItem
{
    public Guid Id {get; private set;}
    public string Title {get; private set;} = string.Empty;

    public string? Description {get; private set;}

    public TaskItemStatus Status {get; private set;}

    public DateTimeOffset CreatedAt {get; private set;}

    public DateTimeOffset? SubmittedAt {get; private set;}


    private TaskItem(Guid id, string title, string? description, DateTimeOffset createdAt)
    {
        Id = id;
        Title = title;
        Description = description;
        CreatedAt = createdAt;
        Status = TaskItemStatus.Draft; 
        SubmittedAt = null;
    }



    public static TaskItem Create(string title, string? description, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(title)) {
            throw new ArgumentException("A title is required to create a draft.", nameof(title));
        }

        Guid id = Guid.NewGuid(); 
 
        return new TaskItem(id, title.Trim(), description, createdAt);

    }

}