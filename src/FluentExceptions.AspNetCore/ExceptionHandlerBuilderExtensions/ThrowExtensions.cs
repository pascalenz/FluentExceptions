using FluentExceptions.AspNetCore.ExceptionHandlerActivities;
using Microsoft.AspNetCore.Http;

namespace FluentExceptions.AspNetCore;

/// <summary>
/// Extension methods for the exception handler builder to replace an exception.
/// </summary>
public static class ThrowExtensions
{
    /// <summary>
    /// Replaces the currently handled exception with another one.
    /// </summary>
    /// <typeparam name="TException">The type of the handled exception.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="action">An action to provide the new exception.</param>
    /// <returns>The exception handler.</returns>
    public static ExceptionHandler Throw<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        Func<HttpContext, TException, Exception> action)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(action);

        return builder
            .AddActivity(new ReplaceExceptionActivity((context, ex) => action(context, (TException)ex)))
            .Create();
    }
}
