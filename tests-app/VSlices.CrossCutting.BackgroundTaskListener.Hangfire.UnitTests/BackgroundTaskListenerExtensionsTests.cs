using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace VSlices.CrossCutting.BackgroundTaskListener.Hangfire.UnitTests;

public class AddHangfireTaskListenerTests
{
    [Fact]
    public void AddHangfireTaskListener_ShouldRegisterInServiceContainer()
    {
        ServiceCollection builder = [];

        builder.AddHangfireTaskListener(config => { });

        builder
            .Where(e => e.ServiceType == typeof(IBackgroundTaskListener))
            .Any(e => e.ImplementationType == typeof(HangfireTaskListener))
            .Should().BeTrue();

        builder
            .Where(e => e.ServiceType == typeof(IHostedService))
            .Any(e => e.ImplementationType == typeof(HangfireTaskListener))
            .Should().BeTrue();

    }
}