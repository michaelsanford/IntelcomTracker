using IntelcomTracker.Models;
using IntelcomTracker.Services;
using IntelcomTracker.Ui;
using Spectre.Console;
using Spectre.Console.Rendering;

namespace IntelcomTracker;

public enum AppView { Dashboard, Detail }
public enum PendingAction { None, Quit, AddPackage, DeletePackage }

public class App(
    ITrackingStoreService persistence,
    RefreshService refreshService,
    TimeProvider? timeProvider = null)
{
    private readonly ITrackingStoreService _persistence = persistence;
    private readonly RefreshService _refreshService = refreshService;
    private readonly TimeProvider _timeProvider = timeProvider ?? TimeProvider.System;

    private const int ManualCooldownSeconds = 300;

    private TrackingStore _store = new();
    private AppView _currentView = AppView.Dashboard;
    private int _selectedIndex;
    private int? _detailIndex;
    private PendingAction _pendingAction = PendingAction.None;
    private bool _forceRefresh;
    private DateTime _lastRefreshed = DateTime.MinValue;

    public async Task RunAsync()
    {
        _store = _persistence.Load();

        if (_store.Packages.Count > 0)
        {
            AnsiConsole.MarkupLine("[grey]Refreshing packages...[/]");
            await _refreshService.RefreshAllAsync(_store, CancellationToken.None);
            _lastRefreshed = _timeProvider.GetUtcNow().UtcDateTime;
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
        var nextAutoRefresh = _timeProvider.GetUtcNow().UtcDateTime.AddSeconds(_store.RefreshIntervalSeconds);

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
                        _lastRefreshed = _timeProvider.GetUtcNow().UtcDateTime;
                        nextAutoRefresh = _timeProvider.GetUtcNow().UtcDateTime.AddSeconds(_store.RefreshIntervalSeconds);
                    }

                    if (_timeProvider.GetUtcNow().UtcDateTime >= nextAutoRefresh)
                    {
                        await _refreshService.RefreshAllAsync(_store, ct);
                        _lastRefreshed = _timeProvider.GetUtcNow().UtcDateTime;
                        nextAutoRefresh = _timeProvider.GetUtcNow().UtcDateTime.AddSeconds(_store.RefreshIntervalSeconds);
                    }

                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        HandleKey(key, cts);
                    }

                    if (!ct.IsCancellationRequested)
                        ctx.UpdateTarget(BuildCurrentRenderable(nextAutoRefresh));

                    try { await Task.Delay(TimeSpan.FromMilliseconds(100), _timeProvider, ct); }
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
                if ((_timeProvider.GetUtcNow().UtcDateTime - _lastRefreshed).TotalSeconds >= ManualCooldownSeconds)
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
                _lastRefreshed.AddSeconds(ManualCooldownSeconds), _timeProvider);

    private async Task HandleAddAsync()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.Markup("[bold]Tracking number:[/] ");
        var trackingId = (Console.ReadLine() ?? "").Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(trackingId))
        {
            AnsiConsole.MarkupLine("[red]No tracking number entered.[/]");
            await Task.Delay(TimeSpan.FromMilliseconds(1200), _timeProvider, CancellationToken.None);
            return;
        }

        if (_store.Packages.Any(p => p.TrackingId.Equals(trackingId, StringComparison.OrdinalIgnoreCase)))
        {
            AnsiConsole.MarkupLine("[yellow]Already tracking that number.[/]");
            await Task.Delay(TimeSpan.FromMilliseconds(1200), _timeProvider, CancellationToken.None);
            return;
        }

        AnsiConsole.Markup("[grey]Nickname (optional, Enter to skip):[/] ");
        var nickname = (Console.ReadLine() ?? "").Trim();

        var pkg = new TrackedPackage
        {
            TrackingId = trackingId,
            Nickname = string.IsNullOrWhiteSpace(nickname) ? null : nickname,
            AddedAt = _timeProvider.GetUtcNow().UtcDateTime
        };

        _store.Packages.Add(pkg);
        _selectedIndex = _store.Packages.Count - 1;
        _persistence.Save(_store);

        AnsiConsole.MarkupLine("[grey]Fetching tracking data...[/]");
        await _refreshService.RefreshAllAsync(_store, CancellationToken.None);
        _lastRefreshed = _timeProvider.GetUtcNow().UtcDateTime;
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
