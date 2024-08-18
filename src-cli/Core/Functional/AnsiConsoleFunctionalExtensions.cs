using LanguageExt.Effects;
using LanguageExt.Traits;
using Spectre.Console;
using VSlices.Base;

namespace Core.Functional;

public static class AnsiConsoleFunctionalExtensions
{
    private static K<Eff, FunctionalAnsiConsole> _trait { get; } = liftEff(() => new FunctionalAnsiConsole());

    private static K<Eff, FunctionalAnsiConsole> trait { get; } =
        Stateful.getsM<Eff, MinRT, FunctionalAnsiConsole>(_ => _trait);

    public static K<Eff, T> Prompt<T>(IPrompt<T> prompt) =>
        from t in trait
        from v in t.Prompt(prompt)
        select v;

    public static K<Eff, Unit> WriteLine(string text) =>
        from t in trait
        from v in t.WriteLine(text)
        select v; 
}
