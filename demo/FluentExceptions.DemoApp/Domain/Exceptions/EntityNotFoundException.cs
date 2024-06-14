namespace FluentExceptions.DemoApp.Domain.Exceptions;

public sealed class EntityNotFoundException(string message)
    : Exception(message)
{ }
