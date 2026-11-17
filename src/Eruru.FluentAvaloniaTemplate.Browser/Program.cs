using Avalonia;
using Avalonia.Browser;
using Eruru.FluentAvaloniaTemplate;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Configs;

internal sealed partial class Program {

	private static async Task Main () {
		await JsInterop.LoadSatelliteAssembliesAsync (
			[.. PresentationApi.AppLanguages.Select (static x => x.Value)]
		).ConfigureAwait (true);
		await BuildAvaloniaApp ()
			.StartBrowserAppAsync ("out").ConfigureAwait (true);
	}

	public static AppBuilder BuildAvaloniaApp () {
		return AppBuilder.Configure<App> ()
			.WithFont_SourceHanSansCN ()
			.AfterSetup (static _ => {
				var services = new ServiceCollection ();
				services.AddHost ();
				services.AddPresentation ();
				services.AddKeyedSingleton<IJsonConfigSource> (
					ConfigIds.Presentation, static (_, _) => new JsonConfigLocalStorageSource ("PresentationConfig")
				);
				PresentationApi.Services = services.BuildServiceProvider ();
			});
	}

}