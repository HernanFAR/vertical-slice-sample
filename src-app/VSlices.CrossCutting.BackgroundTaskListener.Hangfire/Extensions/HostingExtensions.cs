using Hangfire;
using VSlices.CrossCutting.BackgroundTaskListener;
using VSlices.CrossCutting.BackgroundTaskListener.Hangfire;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register <see cref="HangfireTaskListener"/>
/// </summary>
public static class HostingExtensions
{
    /// <summary>
    /// Adds a <see cref="IBackgroundTaskListener"/>
    /// </summary>
    public static IServiceCollection AddHangfireTaskListener(this IServiceCollection services, Action<IGlobalConfiguration> configuration)
        => services.AddHostedService<HangfireTaskListener>()
            .AddSingleton<IBackgroundTaskListener, HangfireTaskListener>()
            .AddHangfireServer()
            .AddHangfire(configuration);
}
