using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Runtime.ExceptionServices;

namespace FluentExceptions.EntityFrameworkCore;

/// <inheritdoc/>
internal sealed class ExceptionHandlerInterceptor(ExceptionHandlingOptions options) : SaveChangesInterceptor
{
    /// <inheritdoc/>
    public override void SaveChangesFailed(DbContextErrorEventData eventData)
    {
        base.SaveChangesFailed(eventData);
        HandleException(eventData.Exception);
    }

    /// <inheritdoc/>
    public override async Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
    {
        await base.SaveChangesFailedAsync(eventData, cancellationToken);
        HandleException(eventData.Exception);
    }

    private void HandleException(Exception exception)
    {
        foreach (var handler in options.Handlers)
        {
            if (!handler.ExceptionType.IsAssignableFrom(exception.GetType()))
                continue;

            if (handler.When != null && !handler.When(exception))
                continue;

            foreach (var interceptor in handler.Interceptors)
            {
                interceptor(exception);
            }

            if (handler.ReplacementExceptionProvider != null)
            {
                exception = handler.ReplacementExceptionProvider(exception);
            }

            if (handler.ThrowExceptionProvider != null)
            {
                exception = handler.ThrowExceptionProvider(exception);
                break;
            }
        }

        ExceptionDispatchInfo.Capture(exception).Throw();
    }
}
