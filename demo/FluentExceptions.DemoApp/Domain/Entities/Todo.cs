using System.ComponentModel.DataAnnotations;

namespace FluentExceptions.DemoApp.Domain.Entities;

public class Todo
{
    private Todo()
    { }

    public int Id { get; private set; }

    public string Title { get; private set; } = "";

    public string Text { get; private set; } = "";

    public DateTime CreatedDateTime { get; private set; } = DateTime.UtcNow;

    public DateTime LastModifiedDateTime { get; private set; } = DateTime.UtcNow;

    public static Todo CreateNew(string title, string text)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException(new ValidationResult("Is required", ["title"]), null, null);

        if (string.IsNullOrWhiteSpace(text))
            throw new ValidationException(new ValidationResult("Is required", ["text"]), null, null);

        return new Todo
        {
            Title = title,
            Text = text,
            CreatedDateTime = DateTime.UtcNow,
            LastModifiedDateTime = DateTime.UtcNow
        };
    }

    public void Update(string? title, string? text)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ValidationException(new ValidationResult("Is required", ["title"]), null, null);

        if (string.IsNullOrWhiteSpace(text))
            throw new ValidationException(new ValidationResult("Is required", ["text"]), null, null);

        Title = title;
        Text = text;
        LastModifiedDateTime = DateTime.UtcNow;
    }
}
