using FluentExceptions.AspNetCore;
using FluentExceptions.EntityFrameworkCore;
using FluentExceptions.DemoApp.Domain.Exceptions;
using FluentExceptions.DemoApp.Domain.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FluentExceptions.DemoApp.Domain;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();

        services.AddDbContext<TodoDataContext>(options => options
            .UseSqlite(connection)
            .UseExceptionManagement(options => options
                .AddHandler(builder => builder
                    .Catch<AggregateException>()
                    .UnwrapInnerException())
                .AddHandler(builder => builder
                    .Catch<DbUpdateException>(ex => ex.InnerException is SqliteException)
                    .UnwrapInnerException())
                .AddHandler(builder => builder
                    .Catch<SqliteException>(ex => ex.SqliteExtendedErrorCode == 787)
                    .Throw(ex => new ReferencedEntityNotFoundException("A referenced entity could not be found.", ex)))
                .AddHandler(builder => builder
                    .Catch<SqliteException>(ex => ex.SqliteExtendedErrorCode == 2067 && ex.Message.Contains("Todo.Title"))
                    .Throw(ex => new EntityUniqueConstraintException("There is already a todo list item with the same title.", "title", ex)))));

        InitializeDatabase(services);

        return services
            .AddSingleton(connection)
            .AddScoped<TodoRepository>();
    }

    private static void InitializeDatabase(IServiceCollection services)
    {
        using var serviceProvider = services.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();
        using var context = scope.ServiceProvider.GetRequiredService<TodoDataContext>();

        context.Database.EnsureCreated();
    }
}
