using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FluentExceptions.AspNetCore;

/// <summary>
/// A builder class for defining an exception handler.
/// </summary>
/// <typeparam name="TException">The type of exception to which the handler applies.</typeparam>
public sealed class ExceptionHandlerBuilder<TException>
    where TException : Exception
{
    private readonly Func<HttpContext, Exception, bool>? when;
    private readonly List<Action<HttpContext, Exception>> interceptors = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlerBuilder{TException}"/> class.
    /// </summary>
    internal ExceptionHandlerBuilder()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlerBuilder{TException}"/> class.
    /// </summary>
    /// <param name="when">The condition under which the handler applies.</param>
    internal ExceptionHandlerBuilder(Func<HttpContext, TException, bool> when)
    {
        ArgumentNullException.ThrowIfNull(when);
        this.when = (context, exception) => when(context, (TException)exception);
    }

    /// <summary>
    /// Defines an action that will be performed before the exception is handled.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The current instance of the builder.</returns>
    public ExceptionHandlerBuilder<TException> Intercept(Action<HttpContext, TException> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        interceptors.Add((context, exception) => action(context, (TException)exception));
        return this;
    }

    /// <summary>
    /// Defines the action that throws a new excaption that replaces the current one.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The exception handler.</returns>
    public ExceptionHandler Throw(Func<HttpContext, TException, Exception> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Exception exceptionProvider(HttpContext context, Exception exception) => action(context, (TException)exception);
        return new ExceptionHandler(typeof(TException), when, interceptors, exceptionProvider, null);
    }

    /// <summary>
    /// Defines the action that handles the exception.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The exception handler.</returns>
    public ExceptionHandler Respond(Func<HttpContext, TException, IActionResult> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        IActionResult resultProvider(HttpContext context, Exception exception) => action(context, (TException)exception);
        return new ExceptionHandler(typeof(TException), when, interceptors, null, resultProvider);
    }
}
