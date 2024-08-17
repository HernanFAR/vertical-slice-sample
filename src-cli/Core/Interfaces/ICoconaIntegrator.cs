using Cocona.Builder;
using VSlices.Base.Core;

namespace Core.Interfaces;

public interface ICoconaIntegrator : IIntegrator
{
    void Define(ICoconaCommandsBuilder builder);
}

