namespace FluentExceptions.DemoApp.Domain.Exceptions;

public sealed class ReferencedEntityNotFoundException(string message, Exception innerException)
    : Exception(message, innerException)
{ }
