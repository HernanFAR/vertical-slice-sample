using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace VSlices.CrossCutting.BackgroundTaskListener.Hosting.UnitTests;

public class AddHostedTaskListenerTests
{
    [Fact]
    public void AddHostedTaskListener_ShouldRegisterInServiceContainer()
    {
        ServiceCollection builder = [];

        builder.AddHostedTaskListener();

        builder
            .Where(e => e.ServiceType == typeof(IBackgroundTaskListener))
            .Any(e => e.ImplementationType == typeof(HostedTaskListener))
            .Should().BeTrue();

    }
}