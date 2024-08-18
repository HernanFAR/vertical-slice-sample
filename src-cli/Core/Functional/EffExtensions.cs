using Spectre.Console;
using VSlices.Base;
using static Core.Functional.AnsiConsoleFunctionalExtensions;

namespace Core.Functional;

public static class EffExtensions
{
    public static Eff<Unit> writeLine(string text) => WriteLine(text).As();

    public static Eff<T> prompt<T>(IPrompt<T> prompt) => Prompt(prompt).As();

    public static Eff<string> promptText(string title) => 
        prompt(new TextPrompt<string>(title));

    public static Eff<string> promptSelection(string title, string[] options) =>
        prompt(new TextPrompt<string>(title).AddChoices(options));

    public static Eff<bool> promptConfirm(string title) =>
        prompt(new ConfirmationPrompt(title));

}