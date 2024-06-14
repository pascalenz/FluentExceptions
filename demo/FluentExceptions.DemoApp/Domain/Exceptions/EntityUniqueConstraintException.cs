namespace FluentExceptions.DemoApp.Domain.Exceptions;

public sealed class EntityUniqueConstraintException(string message, string memberName, Exception innerException)
    : Exception(message, innerException)
{
    public string MemberName => memberName;
}