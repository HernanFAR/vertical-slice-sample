using VSlices.CrossCutting.BackgroundTaskListener;
using VSlices.CrossCutting.BackgroundTaskListener.Hosting;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register <see cref="HostedTaskListener"/>
/// </summary>
public static class HostingExtensions
{
    /// <summary>
    /// Adds a <see cref="IBackgroundTaskListener"/>
    /// </summary>
    public static IServiceCollection AddHostedTaskListener(this IServiceCollection services)
        => services.AddHostedService<HostedTaskListener>()
            .AddSingleton<IBackgroundTaskListener, HostedTaskListener>();
}
