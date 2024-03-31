using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Responses;

namespace VSlices.Core.UseCases.UnitTests.Extensions;

public class SenderExtensionsTests
{
    public class Sender : ISender
    {
        public ValueTask<Result<TResponse>> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }

    [Fact]
    public void AddSender_ShouldAddSender()
    {
        var services = new ServiceCollection();

        services.AddSender<Sender>();

        services
            .Where(e => e.ServiceType == typeof(ISender))
            .Where(e => e.ImplementationType == typeof(Sender))
            .Any(e => e.Lifetime == ServiceLifetime.Scoped)
            .Should().BeTrue();

    }


}
