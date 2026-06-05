using System.Diagnostics;
using Avalonia;
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

namespace Eruru.FluentAvaloniaTemplate {

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
			if (ServiceProvider?.GetRequiredService<JsonConfig<Config, App>> () is not JsonConfig<Config, App> jsonConfig) {
				return;
			}
			if (OperatingSystem.IsBrowser ()) {
				var task = jsonConfig.BuildAsync ();
				_ = task.ContinueWithShowExceptionAsync ();
				_ = task.ContinueWith (static (task, state) => {
					Current = Application.Current as App;
					if (state is not JsonConfig<Config, App> jsonConfig) {
						return;
					}
					jsonConfig.Read ()?.Apply ();
				}, jsonConfig, CancellationToken.None, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
			} else {
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
				jsonConfig.BuildAsync ().GetAwaiter ().GetResult ();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
#pragma warning restore IDE0079 // 请删除不必要的忽略
				Current = this;
				jsonConfig.Read ()?.Apply ();
			}
		}

		public override void OnFrameworkInitializationCompleted () {
			if (ServiceProvider is not ServiceProvider serviceProvider) {
				return;
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
			) {
				return;
			}
			try {
				serviceProvider.GetRequiredService<AppViewModel> ().IsShowTrayIcon = true;
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
			if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) {
				return;
			}
			desktop.Shutdown ();
		}

	}

}