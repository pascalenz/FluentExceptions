using Microsoft.EntityFrameworkCore;
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
            foreach (var activity in handler.Activities)
            {
                var result = activity.Execute(ref exception);

                if (result == ExceptionHandlerActivityResult.Skip)
                    break;

                if (result == ExceptionHandlerActivityResult.Handled)
                    return;
            }
        }

        ExceptionDispatchInfo.Capture(exception).Throw();
    }
}
