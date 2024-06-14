namespace FluentExceptions.EntityFrameworkCore;

/// <summary>
/// Defines an exception handler.
/// </summary>
/// <param name="ExceptionType">The type of excaption to catch.</param>
/// <param name="When">Optional conditions under which to catch the exception.</param>
/// <param name="Interceptors">Actions performed before the exception is handled.</param>
/// <param name="ReplacementExceptionProvider">Provides a replacement exception.</param>
/// <param name="ThrowExceptionProvider">Provides a exception to throw.</param>
public sealed record ExceptionHandler(
    Type ExceptionType,
    Func<Exception, bool>? When,
    IEnumerable<Action<Exception>> Interceptors,
    Func<Exception, Exception>? ReplacementExceptionProvider,
    Func<Exception, Exception>? ThrowExceptionProvider);
