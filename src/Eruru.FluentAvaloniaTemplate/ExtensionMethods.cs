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

namespace Eruru.FluentAvaloniaTemplate {

	public static class ExtensionMethods {

		public static AppBuilder ConfigureServices (this AppBuilder appBuilder, Action<ServiceCollection> action) {
			ArgumentNullException.ThrowIfNull (action, nameof (action));
			var serviceCollection = new ServiceCollection ();
			action (serviceCollection);
			App.ServiceProvider = serviceCollection.BuildServiceProvider ();
			return appBuilder;
		}

		public static void AddCommonServices (this ServiceCollection serviceCollection) {
			serviceCollection.AddSingleton<SplashScreen> ();
			serviceCollection.AddSingleton<NavigationPageFactory> ();
			serviceCollection.AddSingleton<DialogService> ();
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
			var fileInfo = new FileInfo (PathService.ConfigPath);
			fileInfo.Directory?.Create ();
			var jsonSerializerOptions = new JsonSerializerOptions (JsonConfig.JsonConfig.JsonSerializerOptions);
			jsonSerializerOptions.Converters.Add (new ColorJsonConverter ());
			var jsonConfigContext = new JsonConfigContext (jsonSerializerOptions);
#pragma warning disable CA2000 // 丢失范围之前释放对象
			var jsonConfig = new JsonConfig<Config, App> ()
				.ConfigureSource (
					jsonConfigSource ??= new JsonConfigFileSource (fileInfo.FullName, !OperatingSystem.IsIOS ()),
					onSaved: JsonConfig_OnSaved
				)
#pragma warning restore CA2000 // 丢失范围之前释放对象
				.ConfigureValue (static _ => new Config (), jsonConfigContext.Config);
			serviceCollection.AddSingleton (jsonConfig);
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

		static void JsonConfig_OnSaved (object? sender) {
			Debug.WriteLine (nameof (JsonConfig_OnSaved));
		}

	}

}