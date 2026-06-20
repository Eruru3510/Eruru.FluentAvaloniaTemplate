using Avalonia;
using Eruru.FluentAvaloniaTemplate.Resources;
using Eruru.FluentAvaloniaTemplate.Services;

namespace Eruru.FluentAvaloniaTemplate.Desktop;

internal sealed class Program {

	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static async Task Main (string[] args) {
		using var singleInstanceService = new SingleInstanceService ($"{nameof (Eruru)}.{Api.AppId}");
		singleInstanceService.OnReceivedAsync += App.SingleInstanceServiceOnReceivedAsync;
		if (singleInstanceService.IsExists ()) {
			await singleInstanceService.SendAsync (LangKeys.Show).ConfigureAwait (true);
			return;
		}
		BuildAvaloniaApp ()
			.StartWithClassicDesktopLifetime (args);
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp () {
		return AppBuilder.Configure<App> ()
			.UsePlatformDetect ()
#if DEBUG
			.WithDeveloperTools ()
#endif
			.LogToTrace ()
			.AfterPlatformServicesSetup (static appBuilder => {
				appBuilder.ConfigureServices (static (_, serviceCollection) => {
					serviceCollection.AddCommonServices ();
					serviceCollection.AddViews ();
					serviceCollection.AddViewModels ();
					serviceCollection.AddJsonConfig ();
				});
			});
	}

}