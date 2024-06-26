﻿using FluentExceptions.AspNetCore.ExceptionHandlerActivities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace FluentExceptions.AspNetCore;

/// <summary>
/// Extension methods for the exception handler builder to reply with
/// an HTTP status code and optional message.
/// </summary>
public static class ReplyExtensions
{
    private const string TraceIdFieldName = "traceId";
    private const string SingleValidationErrorTitle = "A validation error occurred.";
    private const string MultipleValidationErrorTitle = "Validation errors occurred.";

    /// <summary>
    /// Reply with a custom action.
    /// </summary>
    /// <typeparam name="TException">The type of the handled exception.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="action">An action to provide the response.</param>
    /// <returns>The exception handler.</returns>
    public static ExceptionHandler Reply<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        Func<HttpContext, TException, IActionResult> action)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(action);

        return builder
            .AddActivity(new ReplyActivity((context, ex) => action(context, (TException)ex)))
            .Create();
    }

    /// <summary>
    /// Reply with a redirection response.
    /// </summary>
    /// <typeparam name="TException">The type of the handled exception.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="location">The location the client should redirect to.</param>
    /// <returns>The exception handler builder instance.</returns>
    public static ExceptionHandler Redirect<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        string location)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(builder);
        return Reply(builder, (context, exception) => new RedirectResult(location));
    }

    /// <summary>
    /// Reply with a simple status code response.
    /// </summary>
    /// <typeparam name="TException">The type of the handled exception.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <returns>The exception handler builder instance.</returns>
    public static ExceptionHandler ReplyWithStatusCode<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        HttpStatusCode statusCode)
        where TException : Exception
    {
        return ReplyWithStatusCode(builder, (int)statusCode);
    }

    /// <summary>
    /// Reply with a simple status code response.
    /// </summary>
    /// <typeparam name="TException">The type of the handled exception.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <returns>The exception handler builder instance.</returns>
    public static ExceptionHandler ReplyWithStatusCode<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        int statusCode)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(builder);
        return Reply(builder, (context, exception) => new StatusCodeResult(statusCode));
    }

    /// <summary>
    /// Reply with a status code and a <see cref="ProblemDetails"/> response in accordance with RFC-7807.
    /// </summary>
    /// <typeparam name="TException">The type of the handled exception.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <returns>The exception handler builder instance.</returns>
    public static ExceptionHandler ReplyWithProblemDetails<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        HttpStatusCode statusCode)
        where TException : Exception
    {
        return ReplyWithProblemDetails(builder, (int)statusCode);
    }

    /// <summary>
    /// Reply with a status code and a <see cref="ProblemDetails"/> response in accordance with RFC-7807.
    /// </summary>
    /// <typeparam name="TException">The type of the handled exception.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <returns>The exception handler builder instance.</returns>
    public static ExceptionHandler ReplyWithProblemDetails<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        int statusCode)
        where TException : Exception
    {
        return ReplyWithProblemDetails(builder, statusCode, (context, exception) => new ProblemDetails
        {
            Title = GetProblemDetailTitle(statusCode),
            Status = statusCode,
            Detail = exception.Message
        });
    }

    /// <summary>
    /// Reply with a status code and a <see cref="ProblemDetails"/> response in accordance with RFC-7807.
    /// </summary>
    /// <typeparam name="TException">The type of the handled exception.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="action">An action to generate the problem response message.</param>
    /// <returns>The exception handler builder instance.</returns>
    public static ExceptionHandler ReplyWithProblemDetails<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        HttpStatusCode statusCode,
        Func<HttpContext, TException, ProblemDetails> action)
        where TException : Exception
    {
        return ReplyWithProblemDetails(builder, (int)statusCode, action);
    }

    /// <summary>
    /// Reply with a status code and a <see cref="ProblemDetails"/> response in accordance with RFC-7807.
    /// </summary>
    /// <typeparam name="TException">The type of the handled exception.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="statusCode">The HTTP status code to return.</param>
    /// <param name="action">An action to generate the problem response message.</param>
    /// <returns>The exception handler builder instance.</returns>
    public static ExceptionHandler ReplyWithProblemDetails<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        int statusCode,
        Func<HttpContext, TException, ProblemDetails> action)
        where TException : Exception
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(action);

        return Reply(builder, (context, exception) =>
        {
            var problemDetails = action(context, exception)
                ?? throw new InvalidOperationException("A problem details instance must be returned.");

            problemDetails.Status ??= statusCode;

            if (context.TraceIdentifier != null && !problemDetails.Extensions.ContainsKey(TraceIdFieldName))
                problemDetails.Extensions[TraceIdFieldName] = context.TraceIdentifier;

            return new ObjectResult(problemDetails) { StatusCode = statusCode };
        });
    }

    /// <summary>
    /// Reply with a status code and a <see cref="ValidationProblemDetails"/> response in accordance with RFC-7807.
    /// </summary>
    /// <typeparam name="TException">The type of the handled exception.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <returns>The exception handler builder instance.</returns>
    public static ExceptionHandler ReplyWithValidationProblemDetails<TException>(
        this ExceptionHandlerBuilder<TException> builder)
        where TException : ValidationException
    {
        return ReplyWithValidationProblemDetails(builder, exception => exception.ValidationResult.MemberNames
            .ToDictionary(name => name, name => new[] { exception.ValidationResult.ErrorMessage! }));
    }

    /// <summary>
    /// Reply with a status code and a <see cref="ValidationProblemDetails"/> response in accordance with RFC-7807.
    /// </summary>
    /// <typeparam name="TException">The type of the handled exception.</typeparam>
    /// <param name="builder">The exception handler builder instance.</param>
    /// <param name="errorsFactory">A function to generate the error list.</param>
    /// <returns>The exception handler builder instance.</returns>
    public static ExceptionHandler ReplyWithValidationProblemDetails<TException>(
        this ExceptionHandlerBuilder<TException> builder,
        Func<TException, IDictionary<string, string[]>> errorsFactory)
        where TException : Exception
    {
        return ReplyWithProblemDetails(builder, HttpStatusCode.BadRequest, (context, exception) =>
        {
            var errors = errorsFactory(exception);

            return new ValidationProblemDetails(errors)
            {
                Title = GetProblemDetailTitle(400),
                Status = 400,
                Detail = errors.Count > 1 ? MultipleValidationErrorTitle : SingleValidationErrorTitle,
            };
        });
    }

    private static string GetProblemDetailTitle(int statusCode) => statusCode switch
    {
        400 => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
        401 => "https://tools.ietf.org/html/rfc7231#section-6.5.2",
        403 => "https://tools.ietf.org/html/rfc7231#section-6.5.3",
        404 => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
        405 => "https://tools.ietf.org/html/rfc7231#section-6.5.5",
        406 => "https://tools.ietf.org/html/rfc7231#section-6.5.6",
        408 => "https://tools.ietf.org/html/rfc7231#section-6.5.7",
        409 => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
        415 => "https://tools.ietf.org/html/rfc7232#section-6.5.13",
        412 => "https://tools.ietf.org/html/rfc7232#section-4.2",
        500 => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
        501 => "https://tools.ietf.org/html/rfc7231#section-6.6.2",
        502 => "https://tools.ietf.org/html/rfc7231#section-6.6.3",
        503 => "https://tools.ietf.org/html/rfc7231#section-6.6.4",
        504 => "https://tools.ietf.org/html/rfc7231#section-6.6.5",
        _ => ""
    };
}
