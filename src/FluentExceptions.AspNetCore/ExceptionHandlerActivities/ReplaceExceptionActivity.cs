using Microsoft.AspNetCore.Http;

namespace FluentExceptions.AspNetCore.ExceptionHandlerActivities;

/// <summary>
/// An exception handler activity to replaces the currently handled exception with a new one.
/// </summary>
/// <param name="provider">A function to provide the new exception.</param>
internal sealed class ReplaceExceptionActivity(Func<HttpContext, Exception, Exception> provider) : IExceptionHandlerActivity
{
    /// <inheritdoc />
    public ExceptionHandlerActivityResult Execute(HttpContext httpContext, ref Exception exception)
    {
        exception = provider(httpContext, exception);
        return ExceptionHandlerActivityResult.Skip;
    }
}
