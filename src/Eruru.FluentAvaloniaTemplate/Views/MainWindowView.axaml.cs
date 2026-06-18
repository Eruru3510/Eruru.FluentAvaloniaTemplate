using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Eruru.FluentAvaloniaTemplate.Models;
using Eruru.FluentAvaloniaTemplate.Services;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using Eruru.JsonConfig;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;

namespace Eruru.FluentAvaloniaTemplate.Views;

public partial class MainWindowView : FAAppWindow {

	internal int Counter;

	readonly JsonConfig<Config, App>? JsonConfig;
	readonly AppViewModel? AppViewModel;
	readonly DialogService? DialogService;
	Size WindowSize;

	public MainWindowView () {
		Interlocked.Increment (ref Counter);
		try {
			InitializeComponent ();
			TitleBar.ExtendsContentIntoTitleBar = true;
			JsonConfig = App.ServiceProvider?.GetRequiredService<JsonConfig<Config, App>> ();
			SplashScreen = App.ServiceProvider?.GetRequiredService<SplashScreen> ();
			AppViewModel = App.ServiceProvider?.GetRequiredService<AppViewModel> ();
			DataContext = App.ServiceProvider?.GetRequiredService<MainWindowViewModel> ();
			DialogService = App.ServiceProvider?.GetRequiredService<DialogService> ();
			PropertyChanged += MainWindowView_PropertyChanged;
			if (JsonConfig?.Read () is not Config value) {
				return;
			}
			Width = Math.Max (MinWidth, Math.Min (MaxWidth, value.WindowWidth ?? Width));
			Height = Math.Max (MinHeight, Math.Min (MaxHeight, value.WindowHeight ?? Height));
			WindowSize = new (Width, Height);
			WindowState = value.WindowState ?? WindowState.Normal;
		} catch {
			Interlocked.Decrement (ref Counter);
			throw;
		}
	}

	protected override void OnLoaded (RoutedEventArgs e) {
		try {
			base.OnLoaded (e);
			WindowState = JsonConfig?.Read ()?.WindowState ?? WindowState.Normal;
		} finally {
			Interlocked.Decrement (ref Counter);
		}
	}

	protected override void OnClosing (WindowClosingEventArgs e) {
		ArgumentNullException.ThrowIfNull (e, nameof (e));
		if (e.CloseReason == WindowCloseReason.WindowClosing) {
			e.Cancel = true;
			_ = OnWindowClosingAsync ().ContinueWithShowExceptionAsync ();
			return;
		}
		WindowState = WindowState.Normal;
		Show ();
		if (!OperatingSystem.IsLinux ()) {
			WindowSize = new (Width, Height);
		}
		if (JsonConfig?.Read (
			static (_, value) => new Size (value?.WindowWidth ?? 0, value?.WindowHeight ?? 0)
		) == WindowSize) {
			return;
		}
		JsonConfig?.TryWriteAsync (static (_, value, state) => {
			value.WindowWidth = double.IsFinite (state.WindowSize.Width) ? state.WindowSize.Width : null;
			value.WindowHeight = double.IsFinite (state.WindowSize.Height) ? state.WindowSize.Height : null;
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
		}, this).GetAwaiter ().GetResult ();
		JsonConfig?.TrySaveAsync ().GetAwaiter ().GetResult ();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
	}

	void MainWindowView_PropertyChanged (object? sender, AvaloniaPropertyChangedEventArgs e) {
		if (e.Property != WindowStateProperty || Volatile.Read (ref Counter) > 0 || Design.IsDesignMode
			|| WindowState == WindowState.Minimized || JsonConfig == null
			|| JsonConfig.Read ()?.WindowState == WindowState
		) {
			return;
		}
		_ = JsonConfig.TryWriteAsync (static (_, value, state) => {
			value.WindowState = state.WindowState;
		}, this).ContinueWithShowExceptionAsync ();
	}

	protected override void OnSizeChanged (SizeChangedEventArgs e) {
		base.OnSizeChanged (e);
		if (!OperatingSystem.IsLinux () || WindowState != WindowState.Normal) {
			return;
		}
		WindowSize = new (Width, Height);
	}

	async Task OnWindowClosingAsync () {
		if (DialogService == null || JsonConfig == null) {
			await Api.ShutdownAsync ().ConfigureAwait (false);
			return;
		}
		var isMinimizeToTrayIcon = JsonConfig.Read ()?.IsMinimizeToTrayIcon;
		if (isMinimizeToTrayIcon == null) {
			var viewModel = new AskMinimizeToTrayIconViewModel ();
			if (!await DialogService.ShowAskAsync (new AskMinimizeToTrayIconView () {
				DataContext = viewModel
			}).ConfigureAwait (false)) {
				return;
			}
			isMinimizeToTrayIcon = viewModel.MinimizeToTrayIconType switch {
				MinimizeToTrayIconType.MinimizeToTrayIcon => true,
				MinimizeToTrayIconType.Quit => false,
				_ => throw new NotImplementedException ($"{viewModel.MinimizeToTrayIconType}"),
			};
			if (viewModel.IsDoNotAskAgain) {
				await JsonConfig.TryWriteAsync (static (_, value, state) => {
					value.IsMinimizeToTrayIcon = state;
				}, isMinimizeToTrayIcon).ConfigureAwait (false);
				await JsonConfig.TrySaveAsync ().ConfigureAwait (false);
			}
		}
		if (isMinimizeToTrayIcon.Value) {
			Dispatcher.UIThread.Post (static state => {
				if (state is not MainWindowView mainWindowView) {
					return;
				}
				Interlocked.Increment (ref mainWindowView.Counter);
				try {
					if (OperatingSystem.IsLinux ()) {
						mainWindowView.WindowState = WindowState.Normal;
					}
					mainWindowView.Hide ();
					GC.Collect ();
					mainWindowView.AppViewModel?.IsShowTrayIcon = true;
				} catch {
					Interlocked.Decrement (ref mainWindowView.Counter);
					throw;
				}
			}, this);
			return;
		}
		await Api.ShutdownAsync ().ConfigureAwait (false);
	}

}