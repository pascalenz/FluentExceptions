using Microsoft.AspNetCore.Http;

namespace FluentExceptions.AspNetCore;

/// <summary>
/// An activity to execute of part of an exception handler.
/// </summary>
public interface IExceptionHandlerActivity
{
    /// <summary>
    /// Executes the activity.
    /// </summary>
    /// <param name="httpContext">The current HTTP context.</param>
    /// <param name="exception">The exception to handle.</param>
    /// <returns>The result of the activity.</returns>
    ExceptionHandlerActivityResult Execute(HttpContext httpContext, ref Exception exception);
}
