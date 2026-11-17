using Avalonia;
using Avalonia.iOS;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Configs;

namespace Eruru.FluentAvaloniaTemplate.iOS;

// The UIApplicationDelegate for the application. This class is responsible for launching the 
// User Interface of the application, as well as listening (and optionally responding) to 
// application events from iOS.
[Register ("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
public partial class AppDelegate : AvaloniaAppDelegate<App> {
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix

	protected override AppBuilder CustomizeAppBuilder (AppBuilder builder) {
		return base.CustomizeAppBuilder (builder)
			.AfterSetup (static _ => {
				var services = new ServiceCollection ();
				services.AddHost ();
				services.AddPresentation ();
				services.AddKeyedSingleton<IJsonConfigSource> (
					ConfigIds.Presentation, static (_, _) => new JsonConfigFileSource (Path.Combine (
						Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData),
						"PresentationConfig.json"
					))
				);
				PresentationApi.Services = services.BuildServiceProvider ();
			});
	}

}