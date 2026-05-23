using System.Text;
using IntelcomTracker;
using IntelcomTracker.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

Console.OutputEncoding = Encoding.UTF8;
Console.CursorVisible = false;

var services = new ServiceCollection();
services.AddHttpClient<IIntelcomApiClient, IntelcomApiClient>();
services.AddSingleton<ITrackingStoreService, TrackingStoreService>();
services.AddSingleton<RefreshService>();
services.AddSingleton<App>();

var sp = services.BuildServiceProvider();

try
{
    await sp.GetRequiredService<App>().RunAsync();
}
finally
{
    Console.CursorVisible = true;
    AnsiConsole.Reset();
}
