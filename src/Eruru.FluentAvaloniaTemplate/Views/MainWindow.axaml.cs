using Antelcat.I18N.Avalonia;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using FluentAvalonia.UI.Windowing;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Configs;
using Presentation.Dialogs;
using Presentation.Localization;

namespace Eruru.FluentAvaloniaTemplate.Views;

public partial class MainWindow : FAAppWindow {

	Size WindowSize;

	public MainWindow () {
		DataContext = PresentationApi.Services!.GetRequiredService<MainWindowViewModel> ();
		InitializeComponent ();
		TitleBar.ExtendsContentIntoTitleBar = true;
		Content = PresentationApi.Services!.GetRequiredService<MainView> ();
	}

	protected override void OnInitialized () {
		base.OnInitialized ();
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
		var windowSize = PresentationApi.Services!.GetRequiredService<JsonConfig<PresentationConfig>> ().ReadAsync (
			static x => x.WindowSize
		).GetAwaiter ().GetResult ();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
		if (windowSize != null) {
			Width = windowSize.Width;
			Height = windowSize.Height;
		}
		OnSizeChanged ();
	}

	protected override void OnLoaded (RoutedEventArgs e) {
		base.OnLoaded (e);
		_ = LoadedAsync ().OnFaultedShowDialogAsync ();
	}

	protected override void OnClosing (WindowClosingEventArgs e) {
		base.OnClosing (e);
		ArgumentNullException.ThrowIfNull (e);
		if (e.CloseReason == WindowCloseReason.ApplicationShutdown) {
			return;
		}
		e.Cancel = true;
		_ = AskCloseWindowBehaviourAsync ().OnFaultedShowDialogAsync ();
	}

	void MainWindow_PropertyChanged (object? sender, AvaloniaPropertyChangedEventArgs e) {
		if (e.Property.Name == WindowStateProperty.Name) {
			if (WindowState != WindowState.Minimized) {
				_ = PresentationApi.Services!.GetRequiredService<JsonConfig<PresentationConfig>> ().WriteAsync (
					static (value, state) => value.WindowState = $"{state}", WindowState
				).OnFaultedShowDialogAsync ();
			}
		}
	}

	protected override void OnSizeChanged (SizeChangedEventArgs e) {
		base.OnSizeChanged (e);
		if (OperatingSystem.IsLinux () && WindowState == WindowState.Normal) {
			OnSizeChanged ();
		}
	}

	async Task LoadedAsync (CancellationToken cancellationToken = default) {
		var presentationConfig = await PresentationApi.Services!.GetRequiredService<JsonConfig<PresentationConfig>> ()
			.ReadAsync (cancellationToken).ConfigureAwait (true);
		WindowState = presentationConfig.WindowState switch {
			nameof (WindowState.FullScreen) => WindowState.FullScreen,
			nameof (WindowState.Maximized) => WindowState.Maximized,
			_ => WindowState.Normal
		};
		PropertyChanged += MainWindow_PropertyChanged;
	}

	async Task AskCloseWindowBehaviourAsync (CancellationToken cancellationToken = default) {
		var jsonPresentationConfig = PresentationApi.Services!.GetRequiredService<JsonConfig<PresentationConfig>> ();
		var closeWindowBehaviour = await jsonPresentationConfig.ReadAsync (
			static x => x.CloseWindowBehaviour, cancellationToken
		).ConfigureAwait (true);
		if (string.IsNullOrWhiteSpace (closeWindowBehaviour)) {
			var closeWindowBehaviourView = new CloseWindowBehaviourView ();
			if (await PresentationApi.Services!.GetRequiredService<IDialogService> ().ShowOkCancelAsync (
				closeWindowBehaviourView, I18NExtension.Translate (LangKeys.CloseWindowBehaviour), cancellationToken
			).ConfigureAwait (true) != DialogResult.Primary) {
				return;
			}
			var closeWindowBehaviourViewModel = (CloseWindowBehaviourViewModel)closeWindowBehaviourView.DataContext!;
			closeWindowBehaviour = $"{closeWindowBehaviourViewModel.CloseWindowBehaviour}";
			if (closeWindowBehaviourViewModel.IsDoNotAskAgain) {
				await jsonPresentationConfig.WriteAsync (
					static (value, state) => value.CloseWindowBehaviour = state, closeWindowBehaviour, cancellationToken
				).ConfigureAwait (true);
				PresentationApi.Services!.GetRequiredService<SettingsViewModel> ().CloseWindowBehaviour = closeWindowBehaviour;
			}
		}
		switch (closeWindowBehaviour) {
			case nameof (CloseWindowBehaviour.MinimizeToTray):
				MinimizeWindowToTrayIcon ();
				break;
			case nameof (CloseWindowBehaviour.Quit):
				await ShutdownAsync (cancellationToken).ConfigureAwait (true);
				break;
			default:
				throw new NotImplementedException ($"{closeWindowBehaviour}");
		}
	}

	public void RestoreWindow () {
		Show ();
		if (WindowState == WindowState.Minimized) {
			WindowState = WindowState.Normal;
		}
		Activate ();
	}

	public void MinimizeWindowToTrayIcon () {
		if (OperatingSystem.IsLinux ()) {
			WindowState = WindowState.Normal;
		}
		Hide ();
	}

	public async Task ShutdownAsync (CancellationToken cancellationToken = default) {
		PropertyChanged -= MainWindow_PropertyChanged;
		RestoreWindow ();
		WindowState = WindowState.Normal;
		if (!OperatingSystem.IsLinux ()) {
			OnSizeChanged ();
		}
		await PresentationApi.Services!.GetRequiredService<JsonConfig<PresentationConfig>> ().WriteAsync (
			static (value, state) => value.WindowSize = new (state.Width, state.Height),
			WindowSize, cancellationToken
		).ConfigureAwait (true);
		((IClassicDesktopStyleApplicationLifetime)Application.Current!.ApplicationLifetime!).Shutdown ();
	}

	void OnSizeChanged () {
		if (double.IsFinite (Width) && double.IsFinite (Height)) {
			WindowSize = new (Width, Height);
		}
	}

}