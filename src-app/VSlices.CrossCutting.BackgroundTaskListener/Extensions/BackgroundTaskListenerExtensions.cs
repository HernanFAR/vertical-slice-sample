using VSlices.CrossCutting.BackgroundTaskListener;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register <see cref="IBackgroundTaskListener"/>
/// </summary>
public static class BackgroundTaskListenerExtensions
{
    /// <summary>
    /// Adds a <see cref="IBackgroundTaskListener"/> of type <typeparamref name="T"/>
    /// </summary>
    public static IServiceCollection AddTaskListener<T>(this IServiceCollection services)
        where T : class, IBackgroundTaskListener 
        => services.AddSingleton<IBackgroundTaskListener, T>();

}
