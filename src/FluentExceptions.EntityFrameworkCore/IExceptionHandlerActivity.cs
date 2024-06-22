namespace FluentExceptions.EntityFrameworkCore;

/// <summary>
/// An activity to execute of part of an exception handler.
/// </summary>
public interface IExceptionHandlerActivity
{
    /// <summary>
    /// Executes the activity.
    /// </summary>
    /// <param name="exception">The exception to handle.</param>
    /// <returns>The result of the activity.</returns>
    ExceptionHandlerActivityResult Execute(ref Exception exception);
}
