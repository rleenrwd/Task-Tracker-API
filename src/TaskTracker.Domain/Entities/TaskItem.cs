
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


    public void EditDraft(string title, string? description)
    {
        if (this.Status != TaskItemStatus.Draft)
        {
            throw new InvalidOperationException("The task can only be edited while it is a draft.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("A title is required.", nameof(title));
        }

        this.Title = title.Trim();
        this.Description = description;

    }

    public void Submit(DateTimeOffset submittedAt)
    {
        if (this.Status != TaskItemStatus.Draft)
        {
            throw new InvalidOperationException("The task can only be submitted while it is a draft.");
        }

        if (string.IsNullOrWhiteSpace(this.Title))
        {
            throw new InvalidOperationException("A title is required for submission.");
        }

        if (string.IsNullOrWhiteSpace(this.Description))
        {
            throw new InvalidOperationException("A description is required for submission.");
        }

        this.Status = TaskItemStatus.Submitted;
        this.SubmittedAt = submittedAt;
    }

}