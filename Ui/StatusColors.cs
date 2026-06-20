using Spectre.Console;

namespace IntelcomTracker.Ui;

public static class StatusColors
{
    public static string GetColor(int statusCode, bool isDelivered = false)
    {
        if (isDelivered) return "green";
        return statusCode switch
        {
            0              => "grey",
            105            => "blue",
            108            => "cyan1",
            300            => "yellow",
            >= 400 and < 500 => "green",
            >= 500         => "red",
            _              => "white"
        };
    }

    public static string Colorize(string text, int statusCode, bool isDelivered = false)
        => $"[{GetColor(statusCode, isDelivered)}]{Markup.Escape(text)}[/]";
}
