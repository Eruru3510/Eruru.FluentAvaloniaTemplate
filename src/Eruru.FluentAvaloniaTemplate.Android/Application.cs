using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace Eruru.FluentAvaloniaTemplate.Android;

[Application]
public class Application : AvaloniaAndroidApplication<App> {

	protected Application (nint javaReference, JniHandleOwnership transfer) : base (javaReference, transfer) {

	}

	protected override AppBuilder CustomizeAppBuilder (AppBuilder builder) {
		return base.CustomizeAppBuilder (builder)
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