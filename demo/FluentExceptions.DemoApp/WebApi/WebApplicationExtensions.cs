using FluentExceptions.AspNetCore;
using FluentExceptions.EntityFrameworkCore;
using FluentExceptions.DemoApp.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;

namespace FluentExceptions.DemoApp.WebApi;

public static class WebApplicationExtensions
{
    public static WebApplication UseWebApi(this WebApplication app)
    {
        app.UseExceptionManagement(options => options
            .AddHandler(builder => builder
                .Catch<ValidationException>()
                .RespondWithValidationProblemDetails())
            .AddHandler(builder => builder
                .Catch<EntityUniqueConstraintException>()
                .RespondWithValidationProblemDetails(ex => new Dictionary<string, string[]>() { [ex.MemberName] = [ex.Message] }))
            .AddHandler(builder => builder
                .Catch<ReferencedEntityNotFoundException>()
                .RespondWithProblemDetails(HttpStatusCode.BadRequest))
            .AddHandler(builder => builder
                .Catch<EntityNotFoundException>()
                .RespondWithStatusCode(HttpStatusCode.NotFound))
            .AddHandler(builder => builder
                .Catch<AuthenticationException>()
                .RespondWithStatusCode(HttpStatusCode.Unauthorized))
            .AddHandler(builder => builder
                .Catch<TimeoutException>()
                .Log(LogLevel.Warning)
                .RespondWithStatusCode(HttpStatusCode.GatewayTimeout))
            .AddHandler(builder => builder
                .Catch<SocketException>()
                .Log(LogLevel.Warning)
                .RespondWithStatusCode(HttpStatusCode.BadGateway))
            .AddHandler(builder => builder
                .Catch<NotImplementedException>()
                .Log(LogLevel.Warning)
                .RespondWithStatusCode(HttpStatusCode.NotImplemented))
            .AddHandler(builder => builder
                .Catch<Exception>()
                .Log(LogLevel.Critical)
                .RespondWithStatusCode(HttpStatusCode.InternalServerError)));

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}
