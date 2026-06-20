using Avalonia;
using Avalonia.Browser;
using Eruru.FluentAvaloniaTemplate;
using Eruru.FluentAvaloniaTemplate.Browser;

internal sealed partial class Program {

	private static async Task Main (string[] args) {
		await JsInterop.LoadSatelliteAssembliesAsync ([.. Api.AppLanguages.Select (static x => x.Value)]).ConfigureAwait (true);
		await BuildAvaloniaApp ()
#if DEBUG
			.WithDeveloperTools ()
#endif
			.StartBrowserAppAsync ("out").ConfigureAwait (true);
	}

	public static AppBuilder BuildAvaloniaApp () {
		return AppBuilder.Configure<App> ()
			.WithFont_SourceHanSansCN ()
			.AfterPlatformServicesSetup (static appBuilder => {
				appBuilder.ConfigureServices (static (_, serviceCollection) => {
					serviceCollection.AddCommonServices ();
					serviceCollection.AddViews ();
					serviceCollection.AddViewModels ();
					serviceCollection.AddJsonConfig (new JsonConfigLocalStorageSource ("Config"));
				});
			});
	}

}