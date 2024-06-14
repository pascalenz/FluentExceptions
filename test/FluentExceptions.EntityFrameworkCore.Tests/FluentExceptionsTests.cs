using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FluentExceptions.EntityFrameworkCore.Tests;

public class FluentExceptionsTests
{
    private static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection("Filename=:memory:");
        connection.Open();
        return connection;
    }

    private static TestDataContext CreateTestDataContext(
        SqliteConnection connection,
        Action<ExceptionHandlingOptions> configureExceptionHandler)
    {
        var options = new DbContextOptionsBuilder<TestDataContext>()
            .UseSqlite(connection)
            .UseExceptionManagement(configureExceptionHandler)
            .Options;

        return new TestDataContext(options);
    }

    [Fact]
    public void NoException()
    {
        using var connection = CreateConnection();
        using var context = CreateTestDataContext(connection, options => options
            .AddHandler(builder => builder
                .Catch<Exception>()
                .Throw()));

        context.Database.EnsureCreated();
        context.Add(new TestEntity { Id = 1, Name = "Item 1" });
        context.SaveChanges();
    }

    [Fact]
    public void UnwrapException()
    {
        using var connection = CreateConnection();
        using var context = CreateTestDataContext(connection, options => options
            .AddHandler(builder => builder
                .Catch<DbUpdateException>(ex => ex.InnerException is SqliteException)
                .UnwrapInnerException())
            .AddHandler(builder => builder
                .Catch<SqliteException>(ex => ex.SqliteErrorCode == 1)
                .Throw()));

        var exception = Assert.Throws<SqliteException>(() =>
        {
            context.Add(new TestEntity { Id = 2, Name = "Item 2" });
            context.SaveChanges();
        });
    }

    [Fact]
    public void ReThrowException()
    {
        using var connection = CreateConnection();
        using var context = CreateTestDataContext(connection, options => options
            .AddHandler(builder => builder
                .Catch<DbUpdateException>(ex => ex.InnerException is SqliteException se && se.SqliteErrorCode == 1)
                .Throw()));

        var exception = Assert.Throws<DbUpdateException>(() =>
        {
            context.Add(new TestEntity { Id = 3, Name = "Item 3" });
            context.SaveChanges();
        });

        Assert.Contains(GetType().FullName!, exception.StackTrace);
    }

    [Fact]
    public void ThrowNewException()
    {
        using var connection = CreateConnection();
        using var context = CreateTestDataContext(connection, options => options
            .AddHandler(builder => builder
                .Catch<DbUpdateException>(ex => ex.InnerException is SqliteException se && se.SqliteErrorCode == 1)
                .Throw(ex => ex.InnerException!))
            .AddHandler(builder => builder
                .Catch<DbUpdateException>(ex => ex.InnerException is SqliteException se && se.SqliteErrorCode == 2)
                .Throw(ex => new InvalidOperationException("Wrong When Clause", ex)))
            .AddHandler(builder => builder
                .Catch<DbUpdateException>(ex => ex.InnerException is SqliteException se && se.SqliteErrorCode == 3)
                .Throw(ex => new InvalidOperationException("Wrong When Clause", ex))));

        Assert.Throws<SqliteException>(() =>
        {
            context.Add(new TestEntity { Id = 4, Name = "Item 4" });
            context.SaveChanges();
        });
    }

    [Fact]
    public async Task ThrowNewExceptionAsync()
    {
        using var connection = CreateConnection();
        using var context = CreateTestDataContext(connection, options => options
            .AddHandler(builder => builder
                .Catch<DbUpdateException>(ex => ex.InnerException is SqliteException se && se.SqliteErrorCode == 3)
                .Throw(ex => new InvalidOperationException("Wrong When Clause", ex)))
            .AddHandler(builder => builder
                .Catch<DbUpdateException>(ex => ex.InnerException is SqliteException se && se.SqliteErrorCode == 2)
                .Throw(ex => new InvalidOperationException("Wrong When Clause", ex)))
            .AddHandler(builder => builder
                .Catch<DbUpdateException>(ex => ex.InnerException is SqliteException se && se.SqliteErrorCode == 1)
                .Throw(ex => ex.InnerException!)));

        await Assert.ThrowsAsync<SqliteException>(() =>
        {
            context.Add(new TestEntity { Id = 5, Name = "Item 5" });
            return context.SaveChangesAsync();
        });
    }
}
