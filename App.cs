using IntelcomTracker.Models;
using IntelcomTracker.Services;
using IntelcomTracker.Ui;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace IntelcomTracker;

public enum AppView { Dashboard, Detail }
public enum PendingAction { None, Quit, AddPackage, DeletePackage }

public class App
{
    private readonly ITrackingStoreService _persistence;
    private readonly RefreshService _refreshService;

    private const int ManualCooldownSeconds = 300;

    private TrackingStore _store = new();
    private AppView _currentView = AppView.Dashboard;
    private int _selectedIndex = 0;
    private int? _detailIndex = null;
    private PendingAction _pendingAction = PendingAction.None;
    private bool _forceRefresh = false;
    private DateTime _lastRefreshed = DateTime.MinValue;

    public App(ITrackingStoreService persistence, RefreshService refreshService)
    {
        _persistence = persistence;
        _refreshService = refreshService;
    }

    public async Task RunAsync()
    {
        _store = _persistence.Load();

        if (_store.Packages.Count > 0)
        {
            AnsiConsole.MarkupLine("[grey]Refreshing packages...[/]");
            await _refreshService.RefreshAllAsync(_store, CancellationToken.None);
            _lastRefreshed = DateTime.UtcNow;
        }

        while (true)
        {
            _pendingAction = PendingAction.None;
            _forceRefresh = false;

            await RunLiveLoopAsync();

            switch (_pendingAction)
            {
                case PendingAction.Quit:
                    return;
                case PendingAction.AddPackage:
                    await HandleAddAsync();
                    break;
                case PendingAction.DeletePackage:
                    HandleDelete();
                    break;
            }
        }
    }

    private async Task RunLiveLoopAsync()
    {
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;
        var nextAutoRefresh = DateTime.UtcNow.AddSeconds(_store.RefreshIntervalSeconds);

        await AnsiConsole.Live(BuildCurrentRenderable(nextAutoRefresh))
            .AutoClear(true)
            .Overflow(VerticalOverflow.Ellipsis)
            .Cropping(VerticalOverflowCropping.Bottom)
            .StartAsync(async ctx =>
            {
                while (!ct.IsCancellationRequested)
                {
                    if (_forceRefresh)
                    {
                        _forceRefresh = false;
                        await _refreshService.RefreshAllAsync(_store, ct);
                        _lastRefreshed = DateTime.UtcNow;
                        nextAutoRefresh = DateTime.UtcNow.AddSeconds(_store.RefreshIntervalSeconds);
                    }

                    if (DateTime.UtcNow >= nextAutoRefresh)
                    {
                        await _refreshService.RefreshAllAsync(_store, ct);
                        _lastRefreshed = DateTime.UtcNow;
                        nextAutoRefresh = DateTime.UtcNow.AddSeconds(_store.RefreshIntervalSeconds);
                    }

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        HandleKey(key, cts);
                    }

                    if (!ct.IsCancellationRequested)
                        ctx.UpdateTarget(BuildCurrentRenderable(nextAutoRefresh));

                    try { await Task.Delay(100, ct); }
                    catch (OperationCanceledException) { break; }
                }
            });
    }

    private void HandleKey(ConsoleKeyInfo key, CancellationTokenSource cts)
    {
        switch (key.Key)
        {
            case ConsoleKey.Q:
                _pendingAction = PendingAction.Quit;
                cts.Cancel();
                break;

            case ConsoleKey.A when _currentView == AppView.Dashboard:
                _pendingAction = PendingAction.AddPackage;
                cts.Cancel();
                break;

            case ConsoleKey.D when _currentView == AppView.Dashboard:
                if (_store.Packages.Count > 0)
                {
                    _pendingAction = PendingAction.DeletePackage;
                    cts.Cancel();
                }
                break;

            case ConsoleKey.R:
                if ((DateTime.UtcNow - _lastRefreshed).TotalSeconds >= ManualCooldownSeconds)
                    _forceRefresh = true;
                break;

            case ConsoleKey.UpArrow when _currentView == AppView.Dashboard:
                _selectedIndex = Math.Max(0, _selectedIndex - 1);
                break;

            case ConsoleKey.DownArrow when _currentView == AppView.Dashboard:
                if (_store.Packages.Count > 0)
                    _selectedIndex = Math.Min(_store.Packages.Count - 1, _selectedIndex + 1);
                break;

            case ConsoleKey.Enter when _currentView == AppView.Dashboard:
                if (_store.Packages.Count > 0 && _selectedIndex < _store.Packages.Count)
                {
                    _detailIndex = _selectedIndex;
                    _currentView = AppView.Detail;
                }
                break;

            case ConsoleKey.Escape when _currentView == AppView.Detail:
                _currentView = AppView.Dashboard;
                _detailIndex = null;
                break;
        }
    }

    private IRenderable BuildCurrentRenderable(DateTime nextAutoRefresh = default) =>
        _currentView == AppView.Detail && _detailIndex is { } idx && idx < _store.Packages.Count
            ? DetailView.Build(_store.Packages[idx])
            : DashboardView.Build(_store, _selectedIndex, nextAutoRefresh,
                _lastRefreshed.AddSeconds(ManualCooldownSeconds));

    private async Task HandleAddAsync()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Markup("[bold]Tracking number:[/] ");
        var trackingId = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(trackingId))
        {
            AnsiConsole.MarkupLine("[red]No tracking number entered.[/]");
            await Task.Delay(1200);
            return;
        }

        if (_store.Packages.Any(p => p.TrackingId.Equals(trackingId, StringComparison.OrdinalIgnoreCase)))
        {
            AnsiConsole.MarkupLine("[yellow]Already tracking that number.[/]");
            await Task.Delay(1200);
            return;
        }

        AnsiConsole.Markup("[grey]Nickname (optional, Enter to skip):[/] ");
        var nickname = (Console.ReadLine() ?? "").Trim();

        var pkg = new TrackedPackage
        {
            TrackingId = trackingId,
            Nickname = string.IsNullOrWhiteSpace(nickname) ? null : nickname,
            AddedAt = DateTime.UtcNow
        };

        _store.Packages.Add(pkg);
        _selectedIndex = _store.Packages.Count - 1;
        _persistence.Save(_store);

        AnsiConsole.MarkupLine("[grey]Fetching tracking data...[/]");
        await _refreshService.RefreshAllAsync(_store, CancellationToken.None);
        _lastRefreshed = DateTime.UtcNow;
    }

    private void HandleDelete()
    {
        if (_store.Packages.Count == 0 || _selectedIndex >= _store.Packages.Count)
            return;

        var pkg = _store.Packages[_selectedIndex];
        AnsiConsole.WriteLine();

        var label = pkg.Nickname is not null
            ? $"[bold]{Markup.Escape(pkg.TrackingId)}[/] ({Markup.Escape(pkg.Nickname)})"
            : $"[bold]{Markup.Escape(pkg.TrackingId)}[/]";

        if (!AnsiConsole.Confirm($"Delete {label}?", defaultValue: false))
            return;

        _store.Packages.RemoveAt(_selectedIndex);
        _selectedIndex = Math.Min(_selectedIndex, Math.Max(0, _store.Packages.Count - 1));
        _persistence.Save(_store);
    }
}
