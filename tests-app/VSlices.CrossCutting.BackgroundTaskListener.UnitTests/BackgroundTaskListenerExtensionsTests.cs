using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace VSlices.CrossCutting.BackgroundTaskListener.UnitTests;

public class AddTaskListenerTests
{
    public class TaskListener : IBackgroundTaskListener
    {
        public ValueTask ExecuteRegisteredJobs(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void AddTaskListener_ShouldRegisterInServiceContainer()
    {
        ServiceCollection builder = [];

        builder.AddTaskListener<TaskListener>();

        builder
            .Where(e => e.ServiceType == typeof(IBackgroundTaskListener))
            .Any(e => e.ImplementationType == typeof(TaskListener))
            .Should().BeTrue();

    }
}
