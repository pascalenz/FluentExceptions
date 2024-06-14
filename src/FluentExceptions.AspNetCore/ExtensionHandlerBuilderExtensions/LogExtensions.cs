using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace FluentExceptions.AspNetCore;

/// <summary>
/// Extension methods for the exception handler builder to log exceptions.
/// </summary>
public static class LogExtensions
{
    /// <summary>
    /// Logs the exception using the standard <see cref="ILogger"/>.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to log.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="level">The log level.</param>
    /// <returns>The exception handler builder instance.</returns>
    public static ExceptionHandlerBuilder<TException> Log<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        LogLevel level)
        where TException : Exception
    {
        return Log(builder, level, 0);
    }

    /// <summary>
    /// Logs the exception using the standard <see cref="ILogger"/>.
    /// </summary>
    /// <typeparam name="TException">The type of the exception to log.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="level">The log level.</param>
    /// <param name="eventId">The event identifier.</param>
    /// <returns>The exception handler builder instance.</returns>
    public static ExceptionHandlerBuilder<TException> Log<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        LogLevel level,
        EventId eventId)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(builder);

        return builder.Intercept((context, exception) =>
        {
            var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("FluentExceptions");
            logger.Log(level, eventId, exception, exception.Message);
        });
    }
}
