namespace FluentExceptions.EntityFrameworkCore.ExceptionHandlerActivities;

/// <summary>
/// An exception handler activity to replaces the currently handled exception with a new one.
/// </summary>
/// <param name="provider">A function to provide the new exception.</param>
internal sealed class ReplaceExceptionActivity(Func<Exception, Exception> provider) : IExceptionHandlerActivity
{
    /// <inheritdoc />
    public ExceptionHandlerActivityResult Execute(ref Exception exception)
    {
        exception = provider(exception);
        return ExceptionHandlerActivityResult.Skip;
    }
}
