using FluentAssertions;
using LanguageExt;
using LanguageExt.SysX.Live;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base;
using VSlices.Core.Builder;
using VSlices.CrossCutting.Pipeline.EventFiltering.MessageTemplates;
using VSlices.Domain;
using VSlices.Domain.Interfaces;

namespace VSlices.CrossCutting.Pipeline.EventFiltering.UnitTests.Extensions;

public class EventFilteringBehaviorExtensionsTests
{
    public record EventFilter : IEventFilter<Request>
    {
        public Aff<Runtime, bool> Define(Request @event)
        {
            throw new NotImplementedException();
        }
    }

    public record Request : Event;

    public class CustomTemplate : IEventFilteringMessageTemplate
    {
        public string ContinueExecution { get; }
        public string SkipExecution { get; }
    }

    [Fact]
    public void AddEventFilteringUsing_ShouldRegisterInServiceContainer_DetailWithSpanishTemplate()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddEventFilteringUsing<EventFilter>()
            .UsingSpanishTemplate();

        builder.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Unit>))
            .Any(e => e.ImplementationType == typeof(EventFilteringBehavior<Request>))
            .Should().BeTrue();

        builder.Services
            .Where(e => e.ServiceType == typeof(IEventFilter<Request>))
            .Any(e => e.ImplementationType == typeof(EventFilter))
            .Should().BeTrue();

        builder.Services
            .Where(e => e.ServiceType == typeof(IEventFilteringMessageTemplate))
            .Any(e => e.ImplementationType == typeof(SpanishEventFilteringMessageTemplate))
            .Should().BeTrue();
    }

    [Fact]
    public void AddEventFilteringUsing_ShouldRegisterInServiceContainer_DetailWithEnglishTemplate()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddEventFilteringUsing<EventFilter>()
            .UsingEnglishTemplate();

        builder.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Unit>))
            .Any(e => e.ImplementationType == typeof(EventFilteringBehavior<Request>))
            .Should().BeTrue();

        builder.Services
            .Where(e => e.ServiceType == typeof(IEventFilter<Request>))
            .Any(e => e.ImplementationType == typeof(EventFilter))
            .Should().BeTrue();

        builder.Services
            .Where(e => e.ServiceType == typeof(IEventFilteringMessageTemplate))
            .Any(e => e.ImplementationType == typeof(EnglishEventFilteringMessageTemplate))
            .Should().BeTrue();
    }

    [Fact]
    public void AddEventFilteringUsing_ShouldRegisterInServiceContainer_DetailWithCustomTemplate()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        builder.AddEventFilteringUsing<EventFilter>()
            .UsingTemplate<CustomTemplate>();

        builder.Services
            .Where(e => e.ServiceType == typeof(IPipelineBehavior<Request, Unit>))
            .Any(e => e.ImplementationType == typeof(EventFilteringBehavior<Request>))
            .Should().BeTrue();

        builder.Services
            .Where(e => e.ServiceType == typeof(IEventFilter<Request>))
            .Any(e => e.ImplementationType == typeof(EventFilter))
            .Should().BeTrue();

        builder.Services
            .Where(e => e.ServiceType == typeof(IEventFilteringMessageTemplate))
            .Any(e => e.ImplementationType == typeof(CustomTemplate))
            .Should().BeTrue();
    }

    [Fact]
    public void AddEventFilteringUsing_ShouldThrowExceptoin()
    {
        FeatureBuilder builder = new(new ServiceCollection());

        Func<EventFilteringBehaviorBuilder> act = () => builder.AddEventFilteringUsing(typeof(object));

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage($"{typeof(object).FullName} does not implement {typeof(IEventFilter<>).FullName}");
    }
}
