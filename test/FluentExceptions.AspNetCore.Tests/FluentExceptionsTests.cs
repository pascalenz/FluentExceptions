using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Text.Json;

namespace FluentExceptions.AspNetCore.Tests;

public class FluentExceptionsTests
{
    private static Task<IHost> CreateHost(Func<HttpContext, Task> configureRequestHandler, Action<ExceptionHandlingOptions> configureExceptionHandler)
    {
        return new HostBuilder()
            .ConfigureWebHost(builder => builder
                .UseTestServer()
                .ConfigureServices(services => services.AddMvcCore())
                .Configure(app => app
                    .UseExceptionManagement(configureExceptionHandler)
                    .UseRouting()
                    .UseEndpoints(endpoint => endpoint.MapGet("/test", configureRequestHandler))))
            .StartAsync();
    }

    [Fact]
    public async Task Throw()
    {
        var exception = new AggregateException("Outer Error", new ValidationException("Inner Error"));

        using var host = await CreateHost(
            context => throw exception,
            options => options
                .AddHandler(builder => builder
                    .Catch<AggregateException>((context, exception) => exception.InnerExceptions.Count == 1)
                    .Throw((context, exception) => exception.InnerException!))
                .AddHandler(builder => builder
                    .Catch<ValidationException>()
                    .ReplyWithStatusCode(HttpStatusCode.BadRequest)));

        var response = await host.GetTestClient().GetAsync("/test");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ReplyWithStatusCode()
    {
        var exception = new Exception("Test Error");

        using var host = await CreateHost(
            context => throw exception,
            options => options.AddHandler(builder => builder
                .Catch<Exception>()
                .ReplyWithStatusCode(HttpStatusCode.InternalServerError)));

        var response = await host.GetTestClient().GetAsync("/test");
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
    }

    [Fact]
    public async Task ReplyWithValidationProblemDetailsWithSingleError()
    {
        var exception = new ValidationException(new ValidationResult("Invalid name", ["name"]), null, null);

        using var host = await CreateHost(
            context => throw exception,
            options => options.AddHandler(builder => builder
                .Catch<ValidationException>()
                .ReplyWithValidationProblemDetails()));

        var response = await host.GetTestClient().GetAsync("/test");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = JsonSerializer.Deserialize<ValidationProblemDetails>(await response.Content.ReadAsStringAsync())!;
        Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.5.1", result.Title);
        Assert.Equal("A validation error occurred.", result.Detail);
        Assert.Equal(400, result.Status);
        Assert.Equal(exception.ValidationResult.MemberNames.Count(), result.Errors.Count);
        Assert.Equal(exception.ValidationResult.MemberNames.Single(), result.Errors.Single().Key);
        Assert.Equal(exception.ValidationResult.ErrorMessage, result.Errors.Single().Value.Single());
    }

    [Fact]
    public async Task ReplyWithValidationProblemDetailsWithMultipleErrors()
    {
        var exception = new ValidationException(new ValidationResult("Invalid name", ["key 1", "key 2"]), null, null);

        using var host = await CreateHost(
            context => throw exception,
            options => options.AddHandler(builder => builder
                .Catch<ValidationException>()
                .ReplyWithValidationProblemDetails()));

        var response = await host.GetTestClient().GetAsync("/test");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var result = JsonSerializer.Deserialize<ValidationProblemDetails>(await response.Content.ReadAsStringAsync())!;
        Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.5.1", result.Title);
        Assert.Equal("Validation errors occurred.", result.Detail);
        Assert.Equal(400, result.Status);
        Assert.Equal(exception.ValidationResult.MemberNames.Count(), result.Errors.Count);
        Assert.Equal(exception.ValidationResult.MemberNames.First(), result.Errors.First().Key);
        Assert.Equal(exception.ValidationResult.ErrorMessage, result.Errors.First().Value.First());
        Assert.Equal(exception.ValidationResult.MemberNames.Last(), result.Errors.Last().Key);
        Assert.Equal(exception.ValidationResult.ErrorMessage, result.Errors.Last().Value.First());
    }

    [Fact]
    public async Task ReplyWithDefaultProblemDetails()
    {
        var exception = new ValidationException("Test Error");

        using var host = await CreateHost(
            context => throw exception,
            options => options.AddHandler(builder => builder
                .Catch<ValidationException>()
                .ReplyWithProblemDetails(HttpStatusCode.Conflict)));

        var response = await host.GetTestClient().GetAsync("/test");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var result = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync())!;
        Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.5.8", result.Title);
        Assert.Equal("Test Error", result.Detail);
        Assert.Equal(409, result.Status);
    }

    [Fact]
    public async Task ReplyWithCustomProblemDetails()
    {
        var exception = new ValidationException("Test Error");

        using var host = await CreateHost(
            context => throw exception,
            options => options.AddHandler(builder => builder
                .Catch<ValidationException>()
                .ReplyWithProblemDetails(409, (_, exception) => new ProblemDetails { Title = exception.Message })));

        var response = await host.GetTestClient().GetAsync("/test");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var result = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync())!;
        Assert.Equal(exception.Message, result.Title);
        Assert.Equal(409, result.Status);
    }
}
