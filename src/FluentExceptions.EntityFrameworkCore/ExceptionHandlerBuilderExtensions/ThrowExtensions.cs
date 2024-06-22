using FluentExceptions.EntityFrameworkCore.ExceptionHandlerActivities;

namespace FluentExceptions.EntityFrameworkCore;

/// <summary>
/// Extension methods for the exception handler builder to throw an exception.
/// </summary>
public static class ThrowExtensions
{
    /// <summary>
    /// Defines the action that replaces the current exception with its inner exception.
    /// </summary>
    /// <typeparam name="TException">The type of the exception being handled.</typeparam>
    /// <returns>The exception handler.</returns>
    public static ExceptionHandler UnwrapInnerException<TException>(
        this ExceptionHandlerBuilder<TException> builder)
        where TException : Exception
    {
        return Throw(builder, exception => exception.InnerException
            ?? throw new InvalidOperationException("The exception does not have an inner exception"));
    }

    /// <summary>
    /// Replaces the currently handled exception with another one.
    /// </summary>
    /// <typeparam name="TException">The type of the exception being handled.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="action">An action to provide the new exception.</param>
    /// <returns>The exception handler.</returns>
    public static ExceptionHandler Throw<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        Func<TException, Exception> action)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(action);

        return builder
            .AddActivity(new ReplaceExceptionActivity(ex => action((TException)ex)))
            .Create();
    }

    /// <summary>
    /// Defines the action that replaces the original exception.
    /// </summary>
    /// <typeparam name="TException">The type of the exception being handled.</typeparam>
    /// <param name="action">The action to perform.</param>
    /// <returns>The exception handler.</returns>
    public static ExceptionHandler Throw<TException>(
        this ExceptionHandlerBuilder<TException> builder)
        where TException : Exception
    {
        return builder.Create();
    }
}
