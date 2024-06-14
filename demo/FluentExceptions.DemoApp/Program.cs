using FluentExceptions.DemoApp.Domain;
using FluentExceptions.DemoApp.WebApi;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddDomain()
    .AddWebApi();

var app = builder.Build();
app.UseWebApi();
app.Run();
