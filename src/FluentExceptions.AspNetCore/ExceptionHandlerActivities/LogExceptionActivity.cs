using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentExceptions.AspNetCore.ExceptionHandlerActivities;

/// <summary>
/// An exception handler activity to log the currently handled exception.
/// </summary>
/// <param name="level">The log level.</param>
/// <param name="eventId">The event identifier.</param>
internal sealed class LogExceptionActivity(LogLevel level, EventId? eventId = null) : IExceptionHandlerActivity
{
    /// <inheritdoc />
    public ExceptionHandlerActivityResult Execute(HttpContext httpContext, ref Exception exception)
    {
        var loggerFactory = httpContext.RequestServices.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("FluentExceptions");
        logger.Log(level, eventId ?? 0, exception, exception.Message);
        return ExceptionHandlerActivityResult.Continue;
    }
}
