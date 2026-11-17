using Android.Runtime;
using Avalonia;
using Avalonia.Android;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Configs;

namespace Eruru.FluentAvaloniaTemplate.Android;

[Application]
public class Application : AvaloniaAndroidApplication<App> {

	protected Application (nint javaReference, JniHandleOwnership transfer) : base (javaReference, transfer) {

	}

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