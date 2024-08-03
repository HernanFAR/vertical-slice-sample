using System.Runtime.Serialization;
using LanguageExt;

namespace VSlices.Infrastructure.Domain.EntityFrameworkCore.IntegTests.Models;

public sealed class EntityId : NewType<EntityId, Guid>
{
    public EntityId(Guid value) : base(value) { }

    public EntityId(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public static EntityId Random() => new(Guid.NewGuid());
}