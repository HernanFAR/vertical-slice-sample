using FluentAssertions;
using LanguageExt;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.DependencyInjection;

namespace VSlices.Core.UseCases.UnitTests.Extensions;

public class RequestRunnerExtensionsTests
{
    public class RequestRunner : IRequestRunner
    {
        public ValueTask<Fin<TResult>> RunAsync<TResult>(IRequest<TResult> request, Runtime runtime)
        {
            throw new NotImplementedException();
        }

        public ValueTask<Fin<TResult>> RunAsync<TResult>(IRequest<TResult> request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void AddSender_ShouldAddSender()
    {
        ServiceCollection services = new();

        services.AddRequestRunner<RequestRunner>();

        services
            .Where(e => e.ServiceType == typeof(IRequestRunner))
            .Where(e => e.ImplementationType == typeof(RequestRunner))
            .Any(e => e.Lifetime == ServiceLifetime.Scoped)
            .Should().BeTrue();

    }
}
