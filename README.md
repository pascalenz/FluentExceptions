[![Build & Test](https://github.com/pascalenz/FluentExceptions/actions/workflows/build-and-test.yml/badge.svg)](https://github.com/pascalenz/FluentExceptions/actions/workflows/build-and-test.yml)

# Introduction

## Background
This is a utility library to simplify exception handling in ASP.NET Core applications (in particular Web APIs).

There are a couple of options available in ASP.NET Core to handle exceptions like Exception handler lambda, custom middleware, `IExceptionHandler`, and `IExceptionFilter`.

However, I found myself often writing the same code over and over again in each project and therefore decided to come up with a more reusable approach based on a fluent interface to define common exception handling strategies. It consists of two libraries.

| Library | Purpose |
| --- | --- |
| FluentExceptions.AspNetCore | Provides a fluent interface to define common web exception handling strategies like returning an HTTP error code or a `ProblemDetail` object. |
| FluentExceptions.EntityFrameworkCore | Provides a fluent interface to define common data access exception handling strategies like replacing a generic `SqlException` exception with a more meaningful domain exception based on a specific SQL server error code. |

# Usage Samples

## Handle ASP.NET Core Exceptions
```C#
app.UseExceptionManagement(options => options

    // Handle ValidationException by returning a
    // 400 status code and a ValidationProblemDetails object.
    .AddHandler(builder => builder
        .Catch<ValidationException>()
        .RespondWithValidationProblemDetails())

    // Handle EntityUniqueConstraintException by returning a
    // 400 status code and a ValidationProblemDetails object.
    .AddHandler(builder => builder
        .Catch<EntityUniqueConstraintException>()
        .RespondWithValidationProblemDetails(
            ex => new Dictionary<string, string[]>() { [ex.MemberName] = [ex.Message] }))

    // Handle EntityNotFoundException by returning a 404 status code.
    .AddHandler(builder => builder
        .Catch<EntityNotFoundException>()
        .RespondWithStatusCode(HttpStatusCode.NotFound))

    // Handle AuthenticationException by returning a 401 status code.
    .AddHandler(builder => builder
        .Catch<AuthenticationException>()
        .RespondWithStatusCode(HttpStatusCode.Unauthorized))

    // Handle TimeoutException by returning a 504 status code.
    // In addition, log the error.
    .AddHandler(builder => builder
        .Catch<TimeoutException>()
        .Log(LogLevel.Warning)
        .RespondWithStatusCode(HttpStatusCode.GatewayTimeout))

    // Handle SocketException by returning a 502 status code.
    // In addition, log the error.
    .AddHandler(builder => builder
        .Catch<SocketException>()
        .Log(LogLevel.Warning)
        .RespondWithStatusCode(HttpStatusCode.BadGateway))

    // Handle all other exceptions by returning a 500 status code.
    // In addition, log the error.
    .AddHandler(builder => builder
        .Catch<Exception>()
        .Log(LogLevel.Critical)
        .RespondWithStatusCode(HttpStatusCode.InternalServerError))
);
```

## Handling Entity Framework Core Exceptions

```C#
services.AddDbContext<TodoDataContext>(options => options
    .UseSqlite(connection)
    .UseExceptionManagement(options => options

        // Unwrap AggregateException and continue with next handler.
        .AddHandler(builder => builder
            .Catch<AggregateException>()
            .UnwrapInnerException())

        // Unwrap DbUpdateException and continue with next handler.
        .AddHandler(builder => builder
            .Catch<DbUpdateException>(ex => ex.InnerException is SqliteException)
            .UnwrapInnerException())

        // Handle unique constraint errors and replace them
        // with more meaningful domain exceptions.
        .AddHandler(builder => builder
            .Catch<SqliteException>(ex => ex.SqliteExtendedErrorCode == 2067 &&
                ex.Message.Contains("MyTable.MyField"))
            .Throw(ex => new EntityUniqueConstraintException(
                "There is already a record with the same value.", "MyField", ex)))
    ));
```

## Custom Handling Extension Methods
The libraries contain a common set of handling methods. But you can easily add additional ones by writing extension methods for the `ExceptionHandlerBuilder<TException>` class.

```C#
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

public static class MyExtensions
{
    public static ExceptionHandlerBuilder<TException> Trace<TException>(
        this ExceptionHandlerBuilder<TException> builder)
        where TException : Exception
    {
        return builder.Intercept((httpContext, exception) => Trace.WriteLine(exception));
    }
    
    public static ExceptionHandler Redirect<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        string location)
        where TException : Exception
    {
        return builder.Respond((httpContext, exception) => new RedirectResult(location));
    }
}
```
