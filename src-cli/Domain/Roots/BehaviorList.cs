using System.Collections;
using Domain.Maintainers;
using Domain.ValueObjects;

namespace Domain.Roots;

public sealed class BehaviorList(Language language, IEnumerable<BehaviorName> behaviors) : IEnumerable<BehaviorName>
{
    private readonly List<BehaviorName> _behaviors = behaviors.ToList(); 

    public Language Language { get; } = language;

    public IEnumerator<BehaviorName> GetEnumerator() => _behaviors.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}