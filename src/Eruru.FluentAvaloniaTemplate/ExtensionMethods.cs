using System.Diagnostics;
using System.Text.Json;
using Avalonia;
using Eruru.FluentAvaloniaTemplate.JsonConverters;
using Eruru.FluentAvaloniaTemplate.Models;
using Eruru.FluentAvaloniaTemplate.Services;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using Eruru.FluentAvaloniaTemplate.Views;
using Eruru.JsonConfig;
using Microsoft.Extensions.DependencyInjection;

namespace Eruru.FluentAvaloniaTemplate;

public static class ExtensionMethods {

	public static AppBuilder ConfigureServices (this AppBuilder appBuilder, Action<AppBuilder, ServiceCollection> action) {
		ArgumentNullException.ThrowIfNull (action, nameof (action));
		var serviceCollection = new ServiceCollection ();
		action (appBuilder, serviceCollection);
		App.ServiceProvider = serviceCollection.BuildServiceProvider ();
		return appBuilder;
	}

	public static void AddCommonServices (this ServiceCollection serviceCollection) {
		serviceCollection.AddSingleton<SplashScreen> ();
		serviceCollection.AddSingleton<NavigationService> ();
		serviceCollection.AddSingleton<DialogService> ();
		serviceCollection.AddSingleton<PathService> ();
	}

	public static void AddViews (this ServiceCollection serviceCollection) {
		serviceCollection.AddSingleton<MainWindowView> ();
		serviceCollection.AddSingleton<MainView> ();
		serviceCollection.AddSingleton<SplashScreenView> ();
		serviceCollection.AddSingleton<SettingsPageView> ();
		serviceCollection.AddSingleton<HomePageView> ();
	}

	public static void AddViewModels (this ServiceCollection serviceCollection) {
		serviceCollection.AddSingleton<AppViewModel> ();
		serviceCollection.AddSingleton<MainWindowViewModel> ();
		serviceCollection.AddSingleton<MainViewModel> ();
		serviceCollection.AddSingleton<SplashScreenViewModel> ();
		serviceCollection.AddSingleton<SettingsPageViewModel> ();
		serviceCollection.AddSingleton<HomePageViewModel> ();
	}

	public static void AddJsonConfig (
		this ServiceCollection serviceCollection, IJsonConfigSource? jsonConfigSource = null
	) {
		var jsonSerializerOptions = new JsonSerializerOptions (JsonConfig.JsonConfig.JsonSerializerOptions);
		if (OperatingSystem.IsBrowser ()) {
			jsonSerializerOptions.WriteIndented = false;
		}
		jsonSerializerOptions.Converters.Add (new ColorJsonConverter ());
		var jsonConfigContext = new JsonConfigContext (jsonSerializerOptions);
		serviceCollection.AddSingleton (jsonConfigContext);
		serviceCollection.AddSingleton (provider => {
			var fileInfo = new FileInfo (provider.GetRequiredService<PathService> ().GetConfigPath ());
			fileInfo.Directory?.Create ();
			var jsonConfig = new JsonConfig<Config, App> ()
				.ConfigureSource (
					jsonConfigSource ??= new JsonConfigFileSource (fileInfo.FullName),
					onSaved: JsonConfig_OnSaved
				)
				.ConfigureValue (
					static _ => new Config (),
					App.ServiceProvider!.GetRequiredService<JsonConfigContext> ().Config
				);
			return jsonConfig;
		});
	}

	static void JsonConfig_OnSaved (object? sender) {
		Debug.WriteLine ($"{nameof (JsonConfig_OnSaved)} {nameof (ExtensionMethods)}");
	}

	public static Task ContinueWithShowExceptionAsync (this Task task) {
		ArgumentNullException.ThrowIfNull (task, nameof (task));
		return task.ContinueWith (static task => {
			if (task.Exception == null) {
				return;
			}
			if (App.ServiceProvider?.GetRequiredService<DialogService> () is not DialogService dialogService) {
				Console.WriteLine (task.Exception);
				Debug.WriteLine (task.Exception);
				return;
			}
			_ = dialogService.ShowExceptionAsync (task.Exception).ContinueWithConsoleShowExceptionAsync ();
		}, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
	}

	public static Task ContinueWithConsoleShowExceptionAsync (this Task task) {
		ArgumentNullException.ThrowIfNull (task, nameof (task));
		return task.ContinueWith (static task => {
			Console.WriteLine (task.Exception);
			Debug.WriteLine (task.Exception);
		}, CancellationToken.None, TaskContinuationOptions.OnlyOnFaulted, TaskScheduler.Default);
	}

}