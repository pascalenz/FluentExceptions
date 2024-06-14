using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FluentExceptions.AspNetCore;

/// <summary>
/// Defines an exception handler.
/// </summary>
/// <param name="ExceptionType">The type of excaption to catch.</param>
/// <param name="When">Optional conditions under which to catch the exception.</param>
/// <param name="Interceptors">Actions performed before the exception is handled.</param>
/// <param name="ExceptionProvider">Provide a new exception.</param>
/// <param name="ResultProvider">Provide a result that handles the esxception.</param>
public sealed record ExceptionHandler(
    Type ExceptionType,
    Func<HttpContext, Exception, bool>? When,
    IEnumerable<Action<HttpContext, Exception>> Interceptors,
    Func<HttpContext, Exception, Exception>? ExceptionProvider,
    Func<HttpContext, Exception, IActionResult>? ResultProvider);
