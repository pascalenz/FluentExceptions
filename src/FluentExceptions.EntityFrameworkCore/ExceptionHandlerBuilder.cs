namespace FluentExceptions.EntityFrameworkCore;

/// <summary>
/// A builder class for defining an exception handler.
/// </summary>
/// <typeparam name="TException">The type of exception to which the handler applies.</typeparam>
public sealed class ExceptionHandlerBuilder<TException>
    where TException : Exception
{
    private readonly Func<Exception, bool>? when;
    private readonly List<Action<Exception>> interceptors = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlerBuilder{TException}"/> class.
    /// </summary>
    internal ExceptionHandlerBuilder()
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlerBuilder{TException}"/> class.
    /// </summary>
    /// <param name="when">The condition under which the handler applies.</param>
    internal ExceptionHandlerBuilder(Func<TException, bool> when)
    {
        ArgumentNullException.ThrowIfNull(when);
        this.when = exception => when((TException)exception);
    }

    /// <summary>
    /// Defines an action that will be performed before the exception is handled.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The current instance of the builder.</returns>
    public ExceptionHandlerBuilder<TException> Intercept(Action<TException> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        interceptors.Add(exception => action((TException)exception));
        return this;
    }

    /// <summary>
    /// Defines the action that replaces the current exception with its inner exception.
    /// </summary>
    /// <returns>The exception handler.</returns>
    public ExceptionHandler UnwrapInnerException()
    {
        return Replace(exception => exception.InnerException
            ?? throw new InvalidOperationException("The exception does not have an inner exception"));
    }

    /// <summary>
    /// Defines the action that replaces the original exception.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The exception handler.</returns>
    public ExceptionHandler Replace(Func<TException, Exception> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Exception replacementProvider(Exception exception) => action((TException)exception);
        return new ExceptionHandler(typeof(TException), when, interceptors, replacementProvider, null);
    }

    /// <summary>
    /// Defines the action that replaces the original exception.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The exception handler.</returns>
    public ExceptionHandler Throw()
    {
        return new ExceptionHandler(typeof(TException), when, interceptors, null, null);
    }

    /// <summary>
    /// Defines the action that throws the exception.
    /// </summary>
    /// <param name="action">The action to perform.</param>
    /// <returns>The exception handler.</returns>
    public ExceptionHandler Throw(Func<TException, Exception> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        Exception resultProvider(Exception exception) => action((TException)exception);
        return new ExceptionHandler(typeof(TException), when, interceptors, null, resultProvider);
    }
}
