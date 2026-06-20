using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
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
		e.Cancel = true;
		_ = Async ().ContinueWithShowExceptionAsync ();
		async Task Async () {
			var isClose = true;
			try {
				if (DialogService == null || JsonConfig == null) {
					return;
				}
				if (e.CloseReason == WindowCloseReason.WindowClosing) {
					var isMinimizeToTrayIcon = JsonConfig.Read ()?.IsMinimizeToTrayIcon;
					if (isMinimizeToTrayIcon == null) {
						var viewModel = new AskMinimizeToTrayIconViewModel ();
						if (!await DialogService.ShowAskAsync (new AskMinimizeToTrayIconView () {
							DataContext = viewModel
						}).ConfigureAwait (true)) {
							isClose = false;
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
							}, isMinimizeToTrayIcon).ConfigureAwait (true);
							await JsonConfig.TrySaveAsync ().ConfigureAwait (true);
						}
					}
					if (isMinimizeToTrayIcon.Value) {
						Interlocked.Increment (ref Counter);
						try {
							if (OperatingSystem.IsLinux ()) {
								WindowState = WindowState.Normal;
							}
							Hide ();
							GC.Collect ();
							AppViewModel?.IsShowTrayIcon = true;
						} catch {
							Interlocked.Decrement (ref Counter);
							throw;
						}
						isClose = false;
						return;
					}
					return;
				}
				WindowState = WindowState.Normal;
				Show ();
				if (!OperatingSystem.IsLinux ()) {
					WindowSize = new (Width, Height);
				}
				if (JsonConfig.Read (
					static (_, value) => new Size (value?.WindowWidth ?? 0, value?.WindowHeight ?? 0)
				) == WindowSize) {
					return;
				}
				await JsonConfig.TryWriteAsync (static (_, value, state) => {
					value.WindowWidth = double.IsFinite (state.WindowSize.Width) ? state.WindowSize.Width : null;
					value.WindowHeight = double.IsFinite (state.WindowSize.Height) ? state.WindowSize.Height : null;
				}, this).ConfigureAwait (true);
				await JsonConfig.TrySaveAsync ().ConfigureAwait (true);
			} finally {
				if (isClose) {
					await Api.ShutdownAsync ().ConfigureAwait (true);
				}
			}
		}
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

}