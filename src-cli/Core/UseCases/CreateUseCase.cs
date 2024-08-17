using Cocona;
using Cocona.Builder;
using Core.Interfaces;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Core.UseCases;

// ReSharper disable once CheckNamespace
namespace Core.UseCases.CreateUseCase;

public sealed class CreateUseCaseCommand : IRequest
{
    public static CreateUseCaseCommand Instance { get; } = new();
}

public sealed class CreateUseCaseDependencies : IFeatureDependencies<CreateUseCaseCommand>
{
    public static void DefineDependencies(IFeatureStartBuilder<CreateUseCaseCommand, Unit> feature) =>
        feature.FromIntegration.Using<CoconaIntegrator>()
               .Execute<Handler>();
}

internal sealed class CoconaIntegrator : ICoconaIntegrator
{
    public void Define(ICoconaCommandsBuilder builder) => 
        builder.AddSubCommand("create", subBuilder => subBuilder.AddCommand("use-case", HandleCommand));

    public static void HandleCommand(IRequestRunner runner)
    {
        runner.Run(CreateUseCaseCommand.Instance);
    }
}

internal sealed class Handler : IHandler<CreateUseCaseCommand>
{
    public Eff<VSlicesRuntime, Unit> Define(CreateUseCaseCommand input) =>
        from _ in liftEff(() =>
        {
            Console.WriteLine("Hello world!");
        })
        select unit;
}