using FluentExceptions.EntityFrameworkCore.ExceptionHandlerActivities;

namespace FluentExceptions.EntityFrameworkCore;

/// <summary>
/// A builder class for defining an exception handler.
/// </summary>
/// <typeparam name="TException">The type of exception to which the handler applies.</typeparam>
public sealed class ExceptionHandlerBuilder<TException>
    where TException : Exception
{
    private readonly List<IExceptionHandlerActivity> activities = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlerBuilder{TException}"/> class.
    /// </summary>
    internal ExceptionHandlerBuilder()
    {
        AddActivity(new FilterActivity(ex => typeof(TException).IsAssignableFrom(ex.GetType())));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExceptionHandlerBuilder{TException}"/> class.
    /// </summary>
    /// <param name="when">The condition under which the handler applies.</param>
    internal ExceptionHandlerBuilder(Func<TException, bool> when)
        : this()
    {
        ArgumentNullException.ThrowIfNull(when);
        AddActivity(new FilterActivity(ex => when((TException)ex)));
    }

    /// <summary>
    /// Adds an activity to the exception handler.
    /// </summary>
    /// <param name="activity">The activity to add.</param>
    /// <returns>The exception handler builder.</returns>
    public ExceptionHandlerBuilder<TException> AddActivity(IExceptionHandlerActivity activity)
    {
        ArgumentNullException.ThrowIfNull(activity);
        activities.Add(activity);
        return this;
    }

    /// <summary>
    /// Creates the exception handler with the defined activities.
    /// </summary>
    /// <returns>The exception handler.</returns>
    public ExceptionHandler Create() => new(activities);
}
