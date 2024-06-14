using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace FluentExceptions.AspNetCore;

/// <summary>
/// Provides extension methods for the <see cref="IApplicationBuilder"/> interface
/// to configure Exception Management in an ASP.NET Core application.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Enables Exception Management for the application.
    /// </summary>
    /// <param name="app">The <see cref="IApplicationBuilder"/> instance.</param>
    /// <param name="configure">The action to configure exception management for the application.</param>
    /// <returns>The <see cref="IMvcBuilder"/> instance.</returns>
    public static IApplicationBuilder UseExceptionManagement(this IApplicationBuilder app, Action<ExceptionHandlingOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new ExceptionHandlingOptions();
        configure.Invoke(options);

        return app.UseMiddleware<ExceptionManagementMiddleware>(options);
    }
}
