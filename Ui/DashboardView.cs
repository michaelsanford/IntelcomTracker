using IntelcomTracker.Models;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace IntelcomTracker.Ui;

public static class DashboardView
{
    public static IRenderable Build(TrackingStore store, int selectedIndex,
        DateTime nextAutoRefresh, DateTime manualAvailableAt)
    {
        var table = new Table()
            .RoundedBorder()
            .BorderColor(Color.Grey)
            .Expand();

        table.AddColumn(new TableColumn(new Markup("[grey] # [/]")).Width(4).RightAligned());
        table.AddColumn(new TableColumn(new Markup("[bold]Tracking ID[/]")));
        table.AddColumn(new TableColumn(new Markup("[bold]Nickname[/]")));
        table.AddColumn(new TableColumn(new Markup("[bold]Status[/]")));
        table.AddColumn(new TableColumn(new Markup("[bold]Location[/]")));
        table.AddColumn(new TableColumn(new Markup("[bold]ETA Window[/]")));
        table.AddColumn(new TableColumn(new Markup("[bold]Updated[/]")).Width(10));

        if (store.Packages.Count == 0)
        {
            table.AddRow(
                new Markup(""),
                new Markup("[grey italic]No packages — press [bold white]A[/] to add one.[/]"),
                new Markup(""), new Markup(""), new Markup(""), new Markup(""), new Markup(""));
        }

        for (int i = 0; i < store.Packages.Count; i++)
        {
            var pkg = store.Packages[i];
            var cursor = i == selectedIndex ? "[bold yellow]>[/]" : " ";

            var idMarkup = new Markup($"{cursor} [white]{Markup.Escape(pkg.TrackingId)}[/]");
            var nickMarkup = new Markup(pkg.Nickname is not null
                ? Markup.Escape(pkg.Nickname)
                : "[grey dim]—[/]");

            Markup statusMarkup;
            Markup locationMarkup;
            Markup etaMarkup;

            if (pkg.LastError != null && pkg.CachedData is null)
            {
                statusMarkup = new Markup($"[red]{Markup.Escape(pkg.LastError)}[/]");
                locationMarkup = new Markup("[grey dim]—[/]");
                etaMarkup = new Markup("[grey dim]—[/]");
            }
            else if (pkg.CachedData is null)
            {
                statusMarkup = new Markup("[grey]—[/]");
                locationMarkup = new Markup("[grey dim]—[/]");
                etaMarkup = new Markup("[grey dim]—[/]");
            }
            else
            {
                var last = pkg.CachedData.LastStatus;
                var sc = last?.StatusCode ?? 0;
                var label = last?.Labels?.En?.ShortLabel ?? last?.Label ?? "Unknown";
                statusMarkup = new Markup(StatusColors.Colorize(label, sc));

                var city = last?.PackageLocation?.Address?.City;
                var prov = last?.PackageLocation?.Address?.StateProvince;
                locationMarkup = new Markup(city is not null
                    ? Markup.Escape(prov is not null ? $"{city}, {prov}" : city)
                    : "[grey dim]—[/]");

                etaMarkup = FormatEta(pkg.CachedData.PublicEta);
            }

            var updatedMarkup = new Markup(pkg.LastRefreshed.HasValue
                ? $"[grey]{FormatRelative(pkg.LastRefreshed.Value)}[/]"
                : "[grey dim]Never[/]");

            table.AddRow(
                new Markup($"[grey]{i + 1}[/]"),
                idMarkup, nickMarkup, statusMarkup, locationMarkup, etaMarkup, updatedMarkup);
        }

        var autoRemaining = nextAutoRefresh - DateTime.UtcNow;
        var autoStr = autoRemaining > TimeSpan.Zero
            ? $"   [grey]auto-refresh in {FormatDuration(autoRemaining)}[/]"
            : "";

        var manualCooldown = manualAvailableAt - DateTime.UtcNow;
        var rKey = manualCooldown > TimeSpan.Zero
            ? $"[grey dim][[R]][/] Refresh [grey](in {FormatDuration(manualCooldown)})[/]"
            : "[grey dim][[R]][/] Refresh";

        var footer = new Markup(
            $"[grey dim][[A]][/] Add  [grey dim][[D]][/] Delete  {rKey}  " +
            $"[grey dim][[↑↓]][/] Navigate  [grey dim][[Enter]][/] Details  [grey dim][[Q]][/] Quit" +
            autoStr);

        return new Rows(table, new Padder(footer, new Padding(1, 0, 0, 0)));
    }

    private static Markup FormatEta(PublicEta? eta)
    {
        if (eta is null) return new Markup("[grey dim]—[/]");

        static string T(string? iso) =>
            DateTimeOffset.TryParse(iso, out var dt)
                ? dt.ToLocalTime().ToString("h:mm tt")
                : "?";

        if (eta.From is null && eta.To is null) return new Markup("[grey dim]—[/]");
        return new Markup($"[bold]{T(eta.From)}[/][grey] – [/][bold]{T(eta.To)}[/]");
    }

    private static string FormatRelative(DateTime utc)
    {
        var e = DateTime.UtcNow - utc;
        if (e.TotalSeconds < 60) return $"{(int)e.TotalSeconds}s ago";
        if (e.TotalMinutes < 60) return $"{(int)e.TotalMinutes}m ago";
        return $"{(int)e.TotalHours}h ago";
    }

    private static string FormatDuration(TimeSpan ts)
    {
        if (ts.TotalHours >= 1) return $"{(int)ts.TotalHours}h {ts.Minutes:D2}m";
        if (ts.TotalMinutes >= 1) return $"{(int)ts.TotalMinutes}m {ts.Seconds:D2}s";
        return $"{(int)ts.TotalSeconds}s";
    }
}
