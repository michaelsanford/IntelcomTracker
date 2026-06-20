using IntelcomTracker.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace IntelcomTracker.Ui;

public static class DetailView
{
    public static IRenderable Build(TrackedPackage pkg)
    {
        var data = pkg.CachedData;
        var last = data?.LastStatus;
        var sc = last?.StatusCode ?? 0;
        var shortLabel = last?.Labels?.En?.ShortLabel ?? last?.Label ?? "Unknown";

        var infoGrid = new Grid().Expand();
        infoGrid.AddColumn(new GridColumn());
        infoGrid.AddColumn(new GridColumn());

        infoGrid.AddRow(
            new Markup($"[grey]Tracking ID:[/]  [bold]{Markup.Escape(pkg.TrackingId)}[/]"),
            new Markup(pkg.Nickname is not null
                ? $"[grey]Nickname:[/]  {Markup.Escape(pkg.Nickname)}"
                : ""));

        infoGrid.AddRow(
            new Markup($"[grey]Status:[/]  {StatusColors.Colorize(shortLabel, sc, last?.IsDelivered ?? false)}"),
            new Markup(data?.DriverName is not null
                ? $"[grey]Driver:[/]  [white]{Markup.Escape(data.DriverName)}[/]"
                : ""));

        var etaStr = FormatEtaFull(data?.PublicEta);
        infoGrid.AddRow(
            new Markup($"[grey]ETA Window:[/]  [bold]{Markup.Escape(etaStr)}[/]"),
            new Markup(""));

        var headerPanel = new Panel(infoGrid)
            .Header("[bold] Package Details [/]")
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .Expand();

        var histTable = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .Title("[bold] Event History [/]")
            .Expand();

        histTable.AddColumn(new TableColumn(new Markup("[bold]Time[/]")).Width(24));
        histTable.AddColumn(new TableColumn(new Markup("[bold]Event[/]")));
        histTable.AddColumn(new TableColumn(new Markup("[bold]Location[/]")).Width(22));

        var events = data?.StatusList ?? [];
        foreach (var evt in Enumerable.Reverse(events))
        {
            var time = DateTimeOffset.FromUnixTimeMilliseconds(evt.Timestamp)
                .ToLocalTime()
                .ToString("ddd MMM d, h:mm tt");

            var city = evt.PackageLocation?.Address?.City;
            var prov = evt.PackageLocation?.Address?.StateProvince;
            var loc = city is not null
                ? (prov is not null ? $"{city}, {prov}" : city)
                : "—";

            var label = evt.Labels?.En?.ShortLabel ?? evt.Label ?? "?";

            histTable.AddRow(
                new Markup($"[grey]{Markup.Escape(time)}[/]"),
                new Markup(StatusColors.Colorize(label, evt.StatusCode, evt.IsDelivered)),
                new Markup(Markup.Escape(loc)));
        }

        if (!events.Any())
            histTable.AddRow(new Markup("[grey dim]No history available.[/]"), new Markup(""), new Markup(""));

        var footer = new Markup("[grey dim][[Esc]][/] Back  [grey dim][[R]][/] Refresh");

        return new Rows(headerPanel, histTable, new Padder(footer, new Padding(1, 0, 0, 0)));
    }

    private static string FormatEtaFull(PublicEta? eta)
    {
        if (eta is null) return "—";

        static string T(string? iso) =>
            DateTimeOffset.TryParse(iso, out var dt)
                ? dt.ToLocalTime().ToString("ddd MMM d, h:mm tt")
                : iso ?? "?";

        if (eta.From is null && eta.To is null) return "—";
        return $"{T(eta.From)} – {T(eta.To)}";
    }
}
