using Spectre.Console;

namespace IntelcomTracker.Ui;

public static class StatusColors
{
    public static string GetColor(int statusCode) => statusCode switch
    {
        0              => "grey",
        105            => "blue",
        108            => "cyan1",
        300            => "yellow",
        >= 400 and < 500 => "bright_green",
        >= 500         => "red",
        _              => "white"
    };

    public static string Colorize(string text, int statusCode)
        => $"[{GetColor(statusCode)}]{Markup.Escape(text)}[/]";
}
