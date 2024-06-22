using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FluentExceptions.AspNetCore.ExceptionHandlerActivities;

/// <summary>
/// An exception handler activity that generates a response message from the currently handled exception.
/// </summary>
/// <param name="provider">A function to provide the response message.</param>
internal sealed class ReplyActivity(Func<HttpContext, Exception, IActionResult> provider) : IExceptionHandlerActivity
{
    /// <inheritdoc />
    public ExceptionHandlerActivityResult Execute(HttpContext httpContext, ref Exception exception)
    {
        var actionResult = provider(httpContext, exception);
        var actionContext = new ActionContext { HttpContext = httpContext };
        actionResult.ExecuteResultAsync(actionContext).GetAwaiter().GetResult();
        return ExceptionHandlerActivityResult.Handled;
    }
}
