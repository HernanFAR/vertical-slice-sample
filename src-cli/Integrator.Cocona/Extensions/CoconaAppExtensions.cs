using Core.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using VSlices.Base.Core;

// ReSharper disable once CheckNamespace
namespace Cocona;

public static class CoconaAppExtensions
{
    public static void UseCoconaIntegrators(this CoconaApp app)
    {
        IEnumerable<ICoconaIntegrator> integrators = app.Services
                                                        .GetServices<IIntegrator>()
                                                        .OfType<ICoconaIntegrator>();

        foreach (ICoconaIntegrator integrator in integrators)
        {
            integrator.Define(app);
        }
    }
}
