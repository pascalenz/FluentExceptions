using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Security.Authentication;
using System.Text.Json;

namespace FluentExceptions.AspNetCore.Tests;

public class FluentExceptionsTests
{
    private static IHostBuilder CreateHost(Func<HttpContext, Task> configureRequestHandler, Action<ExceptionHandlingOptions> configureExceptionHandler)
    {
        return new HostBuilder()
            .ConfigureWebHost(builder => builder
                .UseTestServer()
                .ConfigureServices(services => services.AddMvcCore())
                .Configure(app => app
                    .UseExceptionManagement(configureExceptionHandler)
                    .UseRouting()
                    .UseEndpoints(endpoint => endpoint.MapGet("/test", configureRequestHandler))));
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
                    .RespondWithStatusCode(HttpStatusCode.BadRequest)))
            .StartAsync();

        var response = await host.GetTestClient().GetAsync("/test");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task RespondWithValidationProblemDetails_Single()
    {
        var exception = new ValidationException(new ValidationResult("Invalid name", ["name"]), null, null);

        using var host = await CreateHost(
            context => throw exception,
            options => options.AddHandler(builder => builder
                .Catch<ValidationException>()
                .RespondWithValidationProblemDetails()))
            .StartAsync();

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
    public async Task RespondWithValidationProblemDetails_Multiple()
    {
        var exception = new ValidationException(new ValidationResult("Invalid name", ["key 1", "key 2"]), null, null);

        using var host = await CreateHost(
            context => throw exception,
            options => options.AddHandler(builder => builder
                .Catch<ValidationException>()
                .RespondWithValidationProblemDetails()))
            .StartAsync();

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
    public async Task RespondWithProblemDetails_Default()
    {
        var exception = new ValidationException("Test Error");

        using var host = await CreateHost(
            context => throw exception,
            options => options.AddHandler(builder => builder
                .Catch<ValidationException>()
                .RespondWithProblemDetails(HttpStatusCode.Conflict)))
            .StartAsync();

        var response = await host.GetTestClient().GetAsync("/test");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var result = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync())!;
        Assert.Equal("https://tools.ietf.org/html/rfc7231#section-6.5.8", result.Title);
        Assert.Equal("Test Error", result.Detail);
        Assert.Equal(409, result.Status);
    }

    [Fact]
    public async Task RespondWithProblemDetails_Custom()
    {
        var exception = new ValidationException("Test Error");

        using var host = await CreateHost(
            context => throw exception,
            options => options.AddHandler(builder => builder
                .Catch<ValidationException>()
                .RespondWithProblemDetails(409, (_, exception) => new ProblemDetails { Title = exception.Message })))
            .StartAsync();

        var response = await host.GetTestClient().GetAsync("/test");
        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);

        var result = JsonSerializer.Deserialize<ProblemDetails>(await response.Content.ReadAsStringAsync())!;
        Assert.Equal(exception.Message, result.Title);
        Assert.Equal(409, result.Status);
    }
}
