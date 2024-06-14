using Microsoft.AspNetCore.Http;

namespace FluentExceptions.AspNetCore;

/// <summary>
/// A builder class for defining an exception handler's catch condition.
/// </summary>
public sealed class ExceptionCatchBuilder
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionCatchBuilder"/> class.
    /// </summary>
    internal ExceptionCatchBuilder()
    { }

    /// <summary>
    /// Catches exceptions of the suplied type.
    /// </summary>
    /// <typeparam name="TException">The type of exception to catch.</typeparam>
    /// <returns>The exception handler builder for the exception type.</returns>
    public ExceptionHandlerBuilder<TException> Catch<TException>()
        where TException : Exception => new();

    /// <summary>
    /// Catches exceptions of the suplied type when a specific condition applies.
    /// </summary>
    /// <typeparam name="TException">The type of exception to catch.</typeparam>
    /// <param name="when">The condition under which the handler applies.</param>
    /// <returns>The exception handler builder for the exception type.</returns>
    public ExceptionHandlerBuilder<TException> Catch<TException>(Func<HttpContext, TException, bool> when)
        where TException : Exception => new(when);
}
