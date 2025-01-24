using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;

namespace VSlices.Core.UseCases.UnitTests.Extensions;

public class RequestRunnerExtensionsTests
{
    public class RequestRunner : IRequestRunner
    {
        public Fin<TResult1> Run<TResult1>(IInput<TResult1> input, VSlicesRuntime runtime)
        {
            throw new NotImplementedException();
        }

        public Fin<TResult1> Run<TResult1>(IInput<TResult1> input, CancellationToken cancellationToken = default)
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
            .Any(e => e.Lifetime == ServiceLifetime.Singleton)
            .Should().BeTrue();

    }
}
