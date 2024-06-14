using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    /// Handle a request before / after it is processed by the next handler in the pipeline.
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
                if (!handler.ExceptionType.IsAssignableFrom(exception.GetType()))
                    continue;

                if (handler.When != null && !handler.When(httpContext, exception))
                    continue;

                foreach (var interceptor in handler.Interceptors)
                {
                    interceptor(httpContext, exception);
                }

                if (handler.ExceptionProvider != null)
                    exception = handler.ExceptionProvider(httpContext, exception);

                if (handler.ResultProvider != null)
                {
                    var result = handler.ResultProvider(httpContext, exception);
                    await result.ExecuteResultAsync(new ActionContext { HttpContext = httpContext });
                    return;
                }
            }

            ExceptionDispatchInfo.Capture(exception).Throw();
        }
    }
}
