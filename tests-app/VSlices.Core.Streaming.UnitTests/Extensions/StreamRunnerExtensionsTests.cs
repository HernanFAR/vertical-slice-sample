using FluentAssertions;
using LanguageExt;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.DependencyInjection;

namespace VSlices.Core.Stream.UnitTests.Extensions;

public class StreamRunnerExtensionsTests
{
    public class StreamRunner : IStreamRunner
    {
        public ValueTask<Fin<IAsyncEnumerable<TResult>>> RunAsync<TResult>(IStream<TResult> request, Runtime runtime)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Fin<IAsyncEnumerable<TResult>>> RunAsync<TResult>(IStream<TResult> request, CancellationToken cancellationToken)
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
