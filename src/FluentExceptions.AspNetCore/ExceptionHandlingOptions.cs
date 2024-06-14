namespace FluentExceptions.AspNetCore;

/// <summary>
/// Defines the options for the exception handling process.
/// </summary>
public sealed class ExceptionHandlingOptions
{
    private readonly List<ExceptionHandler> handlers = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlingOptions"/> class.
    /// </summary>
    internal ExceptionHandlingOptions()
    { }

    /// <summary>
    /// Gets the definited exception handlers.
    /// </summary>
    internal IEnumerable<ExceptionHandler> Handlers => handlers;

    /// <summary>
    /// Adds an exception handler.
    /// </summary>
    /// <param name="configure">The callback action to configure the exception handler.</param>
    public ExceptionHandlingOptions AddHandler(Func<ExceptionCatchBuilder, ExceptionHandler> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new ExceptionCatchBuilder();
        var handler = configure(builder);
        handlers.Add(handler);
        return this;
    }
}
