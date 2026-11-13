using TaskTracker.Domain.Entities;
using TaskTracker.Domain.Enums;
using Xunit;

public class TaskItemTests
{
    [Fact]
    public void Create_WithValidTitle_ReturnsDraftTask()
    {
        string title = "Test Title";
        string? description = "";
        DateTimeOffset createdAt= DateTimeOffset.UtcNow;

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        Assert.Equal(TaskItemStatus.Draft, newTask.Status);
    }


    [Fact]
    public void Create_WithValidInputs_InitializesProperties()
    {
        string title = "New Test Title";
        string? description = "This is a description for the new test.";
        DateTimeOffset createdAt = new (2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        Assert.Equal(title, newTask.Title);
        Assert.Equal(description, newTask.Description);
        Assert.Equal(createdAt, newTask.CreatedAt);
        Assert.NotEqual(Guid.Empty, newTask.Id);
        Assert.Null(newTask.SubmittedAt);
    }


    [Fact]
    public void Create_WithEmptyTitle_ThrowsArgumentException()
    {
        string title = "";
        string? description = "This is a description";
        DateTimeOffset createdAt = new (2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(() => 
        {
            TaskItem.Create(title, description, createdAt);
        });
    }

    [Fact]
    public void Create_WithWhitespaceTitle_ThrowsArgumentException()
    {
        string title = "    ";
        string? description = "";
        DateTimeOffset createdAt = new (2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(() =>
        {
            TaskItem.Create(title, description, createdAt);
        });
    }

    [Fact]
    public void Create_WithNullTitle_ThrowsArgumentException()
    {
        string? title = null;
        string? description = "This is another test description.";
        DateTimeOffset createdAt = new (2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentException>(() =>
        {
            TaskItem.Create(title!, description, createdAt);
        });
    }


    [Fact]
    public void Create_WithPaddedTitle_TrimsWhitespace()
    {
        string title = "    This is a padded title.          ";
        string? description = null;
        DateTimeOffset createdAt = new (2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        Assert.Equal(title.Trim(), newTask.Title);
    }

    [Fact]
    public void Create_WithNullDescription_ReturnsDraftTask()
    {
        string title = "Valid Title.";
        string? description = null;
        DateTimeOffset createdAt = new (2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        Assert.Null(newTask.Description);
        Assert.Equal(TaskItemStatus.Draft, newTask.Status);
    }

    [Fact]
    public void Create_MultipleTasks_GeneratesUniqueIds()
    {
        string title = "Another Valid Title.";
        string? description = "Another valid description for a task.";
        DateTimeOffset createdAt = new (2026, 11, 8, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask1 = TaskItem.Create(title, description, createdAt);
        TaskItem newTask2 = TaskItem.Create(title, description, createdAt);

        Assert.NotEqual(newTask2.Id, newTask1.Id);
    }


    [Fact]
    public void EditDraft_WithValidInputs_UpdatesProperties()
    {
        string title = "Editing Draft Title";
        string description = "Testing the edit draft functionality.";
        DateTimeOffset createdAt = new (2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

        TaskItem task1 = TaskItem.Create(title, description, createdAt);

        string updatedTitle = "Updating the title";
        string updatedDescription = "Updating the current description to this.";

        task1.EditDraft(updatedTitle, updatedDescription);

        Assert.Equal(updatedTitle, task1.Title);
        Assert.Equal(updatedDescription, task1.Description);
        Assert.Equal(TaskItemStatus.Draft, task1.Status);
    }

    [Fact]
    public void EditDraft_WithEmptyTitle_ThrowsArgumentException()
    {
        string title = "Title";
        string description = "This is another test description.";
        DateTimeOffset createdAt = new (2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        string updatedTitle = "";

        Assert.Throws<ArgumentException>(() =>
        {
            newTask.EditDraft(updatedTitle, newTask.Description);
        });

        Assert.Equal(title, newTask.Title);
    }

    [Fact]
    public void EditDraft_WithWhiteSpaceTitle_ThrowsArgumentException()
    {
        string title = "Title";
        string description = "This is another test description.";
        DateTimeOffset createdAt = new (2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        string updatedTitle = "       ";

        Assert.Throws<ArgumentException>(() =>
        {
            newTask.EditDraft(updatedTitle, newTask.Description);
        });

        Assert.Equal(title, newTask.Title);
    }

    [Fact]
    public void EditDraft_WithNullTitle_ThrowsArgumentException()
    {
        string title = "Title";
        string description = "This is another test description.";
        DateTimeOffset createdAt = new (2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        string? updatedTitle = null;
        Assert.Throws<ArgumentException>(() =>
        {
            newTask.EditDraft(updatedTitle!, newTask.Description);
        });

        Assert.Equal(title, newTask.Title);
    }

    [Fact]
    public void EditDraft_WithPaddedTitle_TrimsWhitespace()
    {
        string title = "Title";
        string description = "This is another test description.";
        DateTimeOffset createdAt = new (2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        string updatedTitle = "      Updated Title           ";

        newTask.EditDraft(updatedTitle, newTask.Description);
        
        Assert.Equal(updatedTitle.Trim(), newTask.Title);
    }

    [Fact]
    public void EditDraft_WithNullDescription_UpdatesDescription()
    {
        string title = "Title";
        string? description = "This is another test description.";
        DateTimeOffset createdAt = new (2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        string? updatedDescription = null;

        newTask.EditDraft(newTask.Title, updatedDescription);
        Assert.Null(updatedDescription);
    }

    [Fact]
    public void EditDraft_WhenAlreadySubmitted_ThrowsInvalidOperationException()
    {
        string title = "Title";
        string? description = "Test description for submit.";
        DateTimeOffset createdAt = new (2026, 11, 11, 11, 5, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        DateTimeOffset submittedAt = new (2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

        newTask.Submit(submittedAt);

        string updatedTitle = "New title";

        Assert.Throws<InvalidOperationException>(() =>
        {
            newTask.EditDraft(updatedTitle, newTask.Description);
        });

        Assert.Equal(title, newTask.Title);

    }

    [Fact]
    public void Submit_WithValidDraft_UpdatesStatusAndTimestamp()
    {
        string title = "Title";
        string? description = "Test description for submit.";
        DateTimeOffset createdAt = new (2026, 11, 11, 11, 5, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        DateTimeOffset submittedAt = new (2026, 11, 11, 12, 0, 0, TimeSpan.Zero);

        newTask.Submit(submittedAt);

        Assert.Equal(TaskItemStatus.Submitted, newTask.Status);
        Assert.Equal(submittedAt, newTask.SubmittedAt);
    }

    [Fact]
    public void Submit_WithNullDescription_ThrowsInvalidOperationException()
    {
        string title = "Title";
        string? description = null;
        DateTimeOffset createdAt = new (2026, 11, 11, 11, 5, 0, TimeSpan.Zero);
        DateTimeOffset submittedAt = new (2026, 11, 12, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        Assert.Throws<InvalidOperationException>(() =>
        {
            newTask.Submit(submittedAt);
        });

        Assert.Equal(TaskItemStatus.Draft, newTask.Status);
        Assert.Null(newTask.SubmittedAt);
    }

    [Fact]
    public void Submit_WithEmptyDescription_ThrowsInvalidOperationException()
    {
        string title = "Title";
        string? description = "";
        DateTimeOffset createdAt = new (2026, 11, 11, 11, 5, 0, TimeSpan.Zero);
        DateTimeOffset submittedAt = new (2026, 11, 12, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        Assert.Throws<InvalidOperationException>(() =>
        {
           newTask.Submit(submittedAt); 
        });

        Assert.Equal(TaskItemStatus.Draft, newTask.Status);
        Assert.Null(newTask.SubmittedAt);
    }

    [Fact]
    public void Submit_WithWhitespaceDescription_ThrowsInvalidOperationException()
    {
        string title = "Title";
        string? description = "     ";
        DateTimeOffset createdAt = new (2026, 11, 11, 11, 5, 0, TimeSpan.Zero);
        DateTimeOffset submittedAt = new (2026, 11, 12, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);

        Assert.Throws<InvalidOperationException>(() =>
        {
            newTask.Submit(submittedAt);
        });

        Assert.Equal(TaskItemStatus.Draft, newTask.Status);
        Assert.Null(newTask.SubmittedAt);
    }

    [Fact]
    public void Submit_WhenAlreadySubmitted_ThrowsInvalidOperationException()
    {
        string title = "Title";
        string? description = "This is a test description";
        DateTimeOffset createdAt = new (2026, 11, 11, 11, 5, 0, TimeSpan.Zero);
        DateTimeOffset submittedAt = new (2026, 11, 12, 12, 0, 0, TimeSpan.Zero);

        TaskItem newTask = TaskItem.Create(title, description, createdAt);
        newTask.Submit(submittedAt);

        DateTimeOffset secondSubmittedTime = new (2026, 11, 12, 13, 0, 0, TimeSpan.Zero);

        Assert.Throws<InvalidOperationException>(() =>
        {
            newTask.Submit(secondSubmittedTime);
        });

        Assert.Equal(TaskItemStatus.Submitted, newTask.Status);
        Assert.Equal(submittedAt, newTask.SubmittedAt);
    }
}

