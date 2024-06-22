using Microsoft.AspNetCore.Http;
using System.Runtime.ExceptionServices;

namespace FluentExceptions.AspNetCore;

/// <summary>
/// Initializes a new instance of the <see cref="ExceptionManagementMiddleware"/> class.
/// </summary>
/// <param name="next">The next middleware componet to call.</param>
/// <param name="options">The exception handling options.</param>
internal sealed class ExceptionManagementMiddleware(RequestDelegate next, ExceptionHandlingOptions options)
{
    private readonly RequestDelegate _next = next;
    private readonly ExceptionHandlingOptions options = options;

    /// <summary>
    /// Handle a request and apply custom exception handling.
    /// </summary>
    /// <param name="httpContext">The HTTP context instance.</param>
    /// <returns>A <see cref="Task" /> representing the asynchronous operation.</returns>
    public async Task Invoke(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        try
        {
            await _next(httpContext);
        }
        catch (Exception exception)
        {
            foreach (var handler in options.Handlers)
            {
                foreach (var activity in handler.Activities)
                {
                    var result = activity.Execute(httpContext, ref exception);

                    if (result == ExceptionHandlerActivityResult.Skip)
                        break;

                    if (result == ExceptionHandlerActivityResult.Handled)
                        return;
                }
            }

            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }
}
