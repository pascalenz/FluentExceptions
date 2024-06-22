namespace FluentExceptions.EntityFrameworkCore.ExceptionHandlerActivities;

/// <summary>
/// An exception handler activity to checks whether the handler applies to the currently handled exception.
/// </summary>
/// <param name="filter">The filter function.</param>
internal sealed class FilterActivity(Func<Exception, bool> filter) : IExceptionHandlerActivity
{
    /// <inheritdoc />
    public ExceptionHandlerActivityResult Execute(ref Exception exception)
    {
        return filter(exception) ? ExceptionHandlerActivityResult.Continue : ExceptionHandlerActivityResult.Skip;
    }
}
