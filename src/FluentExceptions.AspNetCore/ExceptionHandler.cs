namespace FluentExceptions.AspNetCore;

/// <summary>
/// An exception handler consisting of multiple activities.
/// </summary>
/// <param name="Activities">The activities to execute as part of the exception handler.</param>
public sealed record ExceptionHandler(IEnumerable<IExceptionHandlerActivity> Activities);
