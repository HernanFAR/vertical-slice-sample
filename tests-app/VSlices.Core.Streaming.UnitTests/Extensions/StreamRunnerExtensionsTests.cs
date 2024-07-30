using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;

namespace VSlices.Core.Stream.UnitTests.Extensions;

public class StreamRunnerExtensionsTests
{
    public class StreamRunner : IStreamRunner
    {
        public Fin<IAsyncEnumerable<TResult1>> Run<TResult1>(IStream<TResult1> request, VSlicesRuntime runtime)
        {
            throw new NotImplementedException();
        }

        public Fin<IAsyncEnumerable<TResult1>> Run<TResult1>(IStream<TResult1> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void AddSender_ShouldAddSender()
    {
        ServiceCollection services = new();

        services.AddStreamRunner<StreamRunner>();

        services
            .Where(e => e.ServiceType == typeof(IStreamRunner))
            .Where(e => e.ImplementationType == typeof(StreamRunner))
            .Any(e => e.Lifetime == ServiceLifetime.Scoped)
            .Should().BeTrue();

    }
}
