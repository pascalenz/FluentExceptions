using Microsoft.AspNetCore.Http;

namespace FluentExceptions.AspNetCore.ExceptionHandlerActivities;

/// <summary>
/// An exception handler activity to checks whether the handler applies to the currently handled exception.
/// </summary>
/// <param name="filter">The filter function.</param>
internal sealed class FilterActivity(Func<HttpContext, Exception, bool> filter) : IExceptionHandlerActivity
{
    /// <inheritdoc />
    public ExceptionHandlerActivityResult Execute(HttpContext httpContext, ref Exception exception)
    {
        return filter(httpContext, exception) ? ExceptionHandlerActivityResult.Continue : ExceptionHandlerActivityResult.Skip;
    }
}
