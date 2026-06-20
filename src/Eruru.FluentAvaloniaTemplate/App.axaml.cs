using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Eruru.FluentAvaloniaTemplate.Models;
using Eruru.FluentAvaloniaTemplate.Resources;
using Eruru.FluentAvaloniaTemplate.Services;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using Eruru.FluentAvaloniaTemplate.Views;
using Eruru.JsonConfig;
using Microsoft.Extensions.DependencyInjection;

namespace Eruru.FluentAvaloniaTemplate;

public partial class App : Application {

	public static new App? Current { get; private set; }
	public static ServiceProvider? ServiceProvider { get; internal set; }

	static App () {
		Dispatcher.UIThread.UnhandledException += UIThread_UnhandledException;
		TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
		AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
	}

	static void CurrentDomain_UnhandledException (object sender, UnhandledExceptionEventArgs e) {
		if (e.ExceptionObject is not Exception exception) {
			Console.WriteLine (e.ExceptionObject);
			Debug.WriteLine (e.ExceptionObject);
			return;
		}
		PerformShowException (exception, nameof (AppDomain));
	}

	static void TaskScheduler_UnobservedTaskException (object? sender, UnobservedTaskExceptionEventArgs e) {
		e.SetObserved ();
		PerformShowException (e.Exception, nameof (TaskScheduler));
	}

	static void UIThread_UnhandledException (object sender, DispatcherUnhandledExceptionEventArgs e) {
		e.Handled = true;
		PerformShowException (e.Exception, nameof (Dispatcher));
	}

	static void PerformShowException (Exception exception, string title) {
		if (ServiceProvider?.GetRequiredService<DialogService> () is not DialogService dialogService) {
			Console.WriteLine (exception);
			Debug.WriteLine (exception);
			return;
		}
		_ = dialogService.ShowExceptionAsync (exception, title).ContinueWithConsoleShowExceptionAsync ();
	}

	public override void Initialize () {
		AvaloniaXamlLoader.Load (this);
	}

	public override void OnFrameworkInitializationCompleted () {
		if (ServiceProvider is not ServiceProvider serviceProvider
			|| ServiceProvider?.GetRequiredService<JsonConfig<Config, App>> () is not JsonConfig<Config, App> jsonConfig
		) {
			return;
		}
		if (OperatingSystem.IsBrowser ()) {
			_ = Async (jsonConfig).ContinueWithShowExceptionAsync ();
			static async Task Async (JsonConfig<Config, App> jsonConfig) {
				await jsonConfig.BuildAsync ().ConfigureAwait (true);
				Current = Application.Current as App;
			}
		} else {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
			jsonConfig.BuildAsync ().GetAwaiter ().GetResult ();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
			Current = this;
		}
		DataContext = serviceProvider.GetRequiredService<AppViewModel> ();
		switch (ApplicationLifetime) {
			case IClassicDesktopStyleApplicationLifetime desktop: {
				desktop.MainWindow = serviceProvider.GetRequiredService<MainWindowView> ();
				break;
			}
			case IActivityApplicationLifetime singleViewFactoryApplicationLifetime: {
				singleViewFactoryApplicationLifetime.MainViewFactory = () => serviceProvider.GetRequiredService<MainView> ();
				break;
			}
			case ISingleViewApplicationLifetime singleViewPlatform: {
				singleViewPlatform.MainView = serviceProvider.GetRequiredService<MainView> ();
				break;
			}
		}
		jsonConfig.Read ()?.Apply ();
		base.OnFrameworkInitializationCompleted ();
	}

	public static async Task SingleInstanceServiceOnReceivedAsync (SingleInstanceService _, string text) {
		switch (text) {
			case nameof (LangKeys.Show):
				await Dispatcher.UIThread.InvokeAsync (static () => Current?.TrayIcon_Clicked (null, EventArgs.Empty));
				break;
		}
	}

	void TrayIcon_Clicked (object? sender, EventArgs e) {
		if (ServiceProvider is not ServiceProvider serviceProvider
			|| ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop
			|| desktop.MainWindow is not MainWindowView mainWindowView
			|| serviceProvider.GetRequiredService<AppViewModel> () is not AppViewModel appViewModel
			|| serviceProvider.GetRequiredService<JsonConfig<Config, App>> () is not JsonConfig<Config, App> jsonConfig
		) {
			return;
		}
		try {
			appViewModel.IsShowTrayIcon = true;
			mainWindowView.WindowState = jsonConfig.Read ()?.WindowState ?? WindowState.Normal;
			mainWindowView.Show ();
			mainWindowView.Activate ();
		} finally {
			Interlocked.Decrement (ref mainWindowView.Counter);
		}
	}

	void NativeMenuItemShow_Click (object? sender, EventArgs e) {
		TrayIcon_Clicked (null, EventArgs.Empty);
	}

	void NativeMenuItemQuit_Click (object? sender, EventArgs e) {
		_ = Api.ShutdownAsync ().ContinueWithShowExceptionAsync ();
	}

}