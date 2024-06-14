namespace FluentExceptions.DemoApp.WebApi;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWebApi(this IServiceCollection services)
    {
        services.AddControllers();
        return services;
    }
}
