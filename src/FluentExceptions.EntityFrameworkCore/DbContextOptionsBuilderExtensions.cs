using Microsoft.EntityFrameworkCore;
using System.Net;

namespace FluentExceptions.EntityFrameworkCore;

/// <summary>
/// Provides extension methods for the <see cref="DbContextOptionsBuilder"/> interface
/// to configure Exception Management in an ASP.NET Core application.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Enables Exception Management for the application.
    /// </summary>
    /// <param name="builder">The <see cref="DbContextOptionsBuilder"/> instance.</param>
    /// <param name="configure">The action to configure exception management for the application.</param>
    /// <returns>The <see cref="DbContextOptionsBuilder"/> instance.</returns>
    public static DbContextOptionsBuilder UseExceptionManagement(this DbContextOptionsBuilder builder, Action<ExceptionHandlingOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        var options = new ExceptionHandlingOptions();
        configure.Invoke(options);

        builder.AddInterceptors(new ExceptionHandlerInterceptor(options));
        return builder;
    }

    /// <summary>
    /// Enables Exception Management for the application.
    /// </summary>
    /// <typeparam name="TContext">The data context type.</typeparam>
    /// <param name="builder">The <see cref="DbContextOptionsBuilder{TContext}"/> instance.</param>
    /// <param name="configure">The action to configure exception management for the application.</param>
    /// <returns>The <see cref="DbContextOptionsBuilder{TContext}"/> instance.</returns>
    public static DbContextOptionsBuilder<TContext> UseExceptionManagement<TContext>(this DbContextOptionsBuilder<TContext> builder, Action<ExceptionHandlingOptions> configure)
        where TContext : DbContext
    {
        return (DbContextOptionsBuilder<TContext>)UseExceptionManagement((DbContextOptionsBuilder)builder, configure);
    }
}
