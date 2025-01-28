using FluentAssertions;
using LanguageExt;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Base.Definitions;
using VSlices.CrossCutting.Interceptor.Filtering.MessageTemplates;
using VSlices.Domain;

namespace VSlices.CrossCutting.Interceptor.Filtering.UnitTests.Extensions;

public sealed class EventFilteringInterceptorExtensionsTests
{
    public sealed record Filter : IEventFilter<Request, Behavior>
    {
        public Eff<VSlicesRuntime, bool> DefineFilter(Request feature) => throw new NotImplementedException();
    }

    public sealed record Request : Event;

    public sealed class Behavior : IBehavior<Request>
    {
        public Eff<VSlicesRuntime, Unit> Define(Request input) => throw new NotImplementedException();
    }

    public sealed class CustomTemplate : IEventFilteringMessageTemplate
    {
        public string ContinueExecution => string.Empty;
        public string SkipExecution => string.Empty;

    }

    [Fact]
    public void AddEventFilteringUsing_ShouldRegisterInServiceContainer_DetailWithSpanishTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        ServiceCollection services = new();

        InterceptorChain<Request, Unit, Behavior> chain = new(services);

        // Act
        chain.AddFiltering().Using<Filter>().InSpanish();

        // Assert
        services.Where(e => e.ServiceType == typeof(FilteringBehaviorInterceptor<Request, Filter, Behavior>))
                .Any(e => e.Lifetime == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType == typeof(Filter))
                .Any(e => e.Lifetime == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType == typeof(IEventFilteringMessageTemplate))
                .Any(e => e.ImplementationType == typeof(SpanishEventFilteringMessageTemplate))
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(FilteringBehaviorInterceptor<Request, Filter, Behavior>));
    }

    [Fact]
    public void AddEventFilteringUsing_ShouldRegisterInServiceContainer_DetailWithEnglishTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        ServiceCollection services = new();

        InterceptorChain<Request, Unit, Behavior> chain = new(services);

        // Act
        chain.AddFiltering().Using<Filter>().InEnglish();

        // Assert
        services.Where(e => e.ServiceType == typeof(FilteringBehaviorInterceptor<Request, Filter, Behavior>))
                .Any(e => e.Lifetime == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType == typeof(Filter))
                .Any(e => e.Lifetime == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType == typeof(IEventFilteringMessageTemplate))
                .Any(e => e.ImplementationType == typeof(EnglishEventFilteringMessageTemplate))
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(FilteringBehaviorInterceptor<Request, Filter, Behavior>));
    }

    [Fact]
    public void AddEventFilteringUsing_ShouldRegisterInServiceContainer_DetailWithCustomTemplate()
    {
        // Arrange
        const int expBehaviorCount = 1;

        ServiceCollection services = new();

        InterceptorChain<Request, Unit, Behavior> chain = new(services);

        // Act
        chain.AddFiltering().Using<Filter>().In<CustomTemplate>();

        // Assert
        services.Where(e => e.ServiceType == typeof(FilteringBehaviorInterceptor<Request, Filter, Behavior>))
                .Any(e => e.Lifetime == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType == typeof(Filter))
                .Any(e => e.Lifetime == ServiceLifetime.Transient)
                .Should().BeTrue();

        services.Where(e => e.ServiceType == typeof(IEventFilteringMessageTemplate))
                .Any(e => e.ImplementationType == typeof(CustomTemplate))
                .Should().BeTrue();

        chain.Behaviors.Should()
             .HaveCount(expBehaviorCount)
             .And.Contain(type => type == typeof(FilteringBehaviorInterceptor<Request, Filter, Behavior>));
    }
}