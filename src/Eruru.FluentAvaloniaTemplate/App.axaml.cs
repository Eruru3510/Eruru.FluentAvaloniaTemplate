using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using Eruru.FluentAvaloniaTemplate.Views;
using Microsoft.Extensions.DependencyInjection;
using Presentation;

namespace Eruru.FluentAvaloniaTemplate;

public class App : Application {

	static App () {
		PresentationApi.RegisterGlobalExceptionHandler ();
	}

	public override void Initialize () {
		AvaloniaXamlLoader.Load (this);
#if DEBUG
		this.AttachDeveloperTools ();
#endif
	}

	public override void OnFrameworkInitializationCompleted () {
		DataContext = PresentationApi.Services!.GetRequiredService<AppViewModel> ();
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) {
			desktop.MainWindow = PresentationApi.Services!.GetRequiredService<MainWindow> ();
		} else if (ApplicationLifetime is IActivityApplicationLifetime singleViewFactoryApplicationLifetime) {
			singleViewFactoryApplicationLifetime.MainViewFactory = static () => PresentationApi.Services!.GetRequiredService<MainView> ();
		} else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform) {
			singleViewPlatform.MainView = PresentationApi.Services!.GetRequiredService<MainView> ();
		} else {
			throw new NotImplementedException ($"{ApplicationLifetime}");
		}
		base.OnFrameworkInitializationCompleted ();
	}

	void TrayIcon_Clicked (object? sender, EventArgs e) {
		PresentationApi.Services!.GetRequiredService<MainWindow> ().RestoreWindow ();
	}

	void NativeMenuItemShow_Click (object? sender, EventArgs e) {
		PresentationApi.Services!.GetRequiredService<MainWindow> ().RestoreWindow ();
	}

	void NativeMenuItemQuit_Click (object? sender, EventArgs e) {
		_ = PresentationApi.Services!.GetRequiredService<MainWindow> ().ShutdownAsync ().OnFaultedShowDialogAsync ();
	}

}