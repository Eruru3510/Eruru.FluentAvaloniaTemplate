using Avalonia;
using Consolonia;
using Consolonia.ManagedWindows.Storage;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Configs;

namespace Eruru.FluentAvaloniaTemplate.Consolonia;

public static class Program {

	[STAThread]
	private static void Main (string[] args) {
		BuildAvaloniaApp ()
			.StartWithConsoleLifetime (args);
	}

	public static AppBuilder BuildAvaloniaApp () {
		return AppBuilder.Configure<App> ()
			.UseConsolonia ()
			.UseAutoDetectedConsole ()
			.LogToException ()
			.UseConsoloniaStorage ()
			.AfterSetup (static _ => {
				var services = new ServiceCollection ();
				services.AddHost ();
				services.AddPresentation ();
				services.AddKeyedSingleton<IJsonConfigSource> (
					ConfigIds.Presentation, static (_, _) => new JsonConfigFileSource (Path.Combine (
						Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData),
						"Eruru", "FluentAvaloniaTemplate", "PresentationConfig.json"
					))
				);
				PresentationApi.Services = services.BuildServiceProvider ();
			});
	}

}