using Spectre.Console;

namespace Core.Functional;

public sealed class FunctionalAnsiConsole
{
    public IO<T> Prompt<T>(IPrompt<T> prompt) => 
        lift(() => AnsiConsole.Prompt(prompt));

    public IO<Unit> WriteLine(string text) =>
        lift(() => AnsiConsole.WriteLine(text));
}