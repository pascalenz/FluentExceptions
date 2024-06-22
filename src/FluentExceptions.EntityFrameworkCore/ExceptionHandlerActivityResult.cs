namespace FluentExceptions.EntityFrameworkCore;

/// <summary>
/// Specifies the result of an exception handler activity.
/// </summary>
public enum ExceptionHandlerActivityResult
{
    /// <summary>
    /// Continue with the next activity in the handler.
    /// </summary>
    Continue,

    /// <summary>
    /// Skip the remaining activities in the handler and move to the next handler.
    /// </summary>
    Skip,

    /// <summary>
    /// The exception has been hanlded by the activity.
    /// </summary>
    Handled
}
