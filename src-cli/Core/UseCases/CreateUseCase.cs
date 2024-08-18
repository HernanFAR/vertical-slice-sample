using System.Diagnostics;
using Cocona;
using Cocona.Builder;
using Core.Functional;
using Core.Interfaces;
using Domain.Maintainers;
using Domain.ValueObjects;
using LanguageExt.Effects;
using VSlices.Base;
using VSlices.Base.Builder;
using VSlices.Base.Core;
using VSlices.Core.UseCases;

// ReSharper disable once CheckNamespace
namespace Core.UseCases.CreateUseCase;

public sealed record CreateUseCaseCommand(FeatureName Name, FeatureType Type, PathString Path, bool IncludeResponse)
    : IRequest;

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
        var eff = from name in promptText("Enter [green]use-case[/] name:")
                  from _1 in writeLine($"Yeah, {name} is a good name")
                  from type in promptSelection("What's the [green]type[/] of the use case?",
                                               ["Query", "Command"])
                  from _2 in writeLine($"Interesting... so {type} it is!")
                  from includeResponse in type switch
                  {
                      "Command" => promptConfirm("So, you need a dto for the response?"),
                      "Query"   => lift(() => true),
                      _         => throw new UnreachableException("This should never happen")
                  }
                  from _3 in writeLine($"You {(includeResponse ? "" : "don't ")}need a dto for the response")
                  from path in promptText("Where should it be created?")
                  from command in liftEff(() => new CreateUseCaseCommand(FeatureName.New(name),
                                                                         FeatureType.New(type),
                                                                         PathString.New(path),
                                                                         includeResponse))
                  from _4 in runner.Run(command)
                                   .ToEff()
                  select unit;

        _ = eff.Run(new MinRT(EnvIO.New()));
    }
}

internal sealed class Handler : IHandler<CreateUseCaseCommand>
{
    public Eff<VSlicesRuntime, Unit> Define(CreateUseCaseCommand input) =>
        from _1 in writeLine("Creating use case...")
        from _2 in writeLine($"Name: {input.Name}") 
        from _3 in writeLine($"Type: {input.Type}")
        from _4 in writeLine($"Include Response: {input.IncludeResponse}")
        select unit;
}