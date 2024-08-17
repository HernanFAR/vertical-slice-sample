using Domain.Maintainers;
using Domain.ValueObjects;
using VSlices.Domain;

namespace Domain.Roots;

public abstract class FeatureDefinition(FeatureName name, BehaviorList behaviors) : AggregateRoot<FeatureName>(name)
{
    public FeatureName Name => Id;

    public BehaviorList Behaviors { get; } = behaviors;

    public abstract FeatureType Type { get; }

    public abstract bool IncludeResponse { get; }
}

public sealed class UseCaseDefinition(FeatureName name, FeatureType type, BehaviorList behaviors, bool includeResponse)
    : FeatureDefinition(name, behaviors)
{
    public override FeatureType Type => type;

    public override bool IncludeResponse => includeResponse;
}

