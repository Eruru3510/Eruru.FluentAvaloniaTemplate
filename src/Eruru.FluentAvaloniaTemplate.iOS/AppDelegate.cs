using Avalonia;
using Avalonia.iOS;

namespace Eruru.FluentAvaloniaTemplate.iOS {

	// The UIApplicationDelegate for the application. This class is responsible for launching the 
	// User Interface of the application, as well as listening (and optionally responding) to 
	// application events from iOS.
	[Register ("AppDelegate")]
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix
#pragma warning disable CA1515 // 考虑将公共类型设为内部类型
	public partial class AppDelegate : AvaloniaAppDelegate<App> {
#pragma warning restore CA1515 // 考虑将公共类型设为内部类型
#pragma warning restore CA1711 // Identifiers should not have incorrect suffix

		protected override AppBuilder CustomizeAppBuilder (AppBuilder builder) {
			return base.CustomizeAppBuilder (builder)
				.AfterPlatformServicesSetup (static appBuilder => {
					appBuilder.ConfigureServices (static serviceCollection => {
						serviceCollection.AddCommonServices ();
						serviceCollection.AddViews ();
						serviceCollection.AddViewModels ();
						serviceCollection.AddJsonConfig ();
					});
				});
		}

	}

}