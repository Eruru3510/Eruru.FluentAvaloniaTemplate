using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Eruru.FluentAvaloniaTemplate.Consolonia.Views;
using Microsoft.Extensions.DependencyInjection;
using Presentation;

namespace Eruru.FluentAvaloniaTemplate.Consolonia;

public partial class App : Application {

	static App () {
		PresentationApi.RegisterGlobalExceptionHandler ();
	}

	public override void Initialize () {
		AvaloniaXamlLoader.Load (this);
	}

	public override void OnFrameworkInitializationCompleted () {
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime) {
			desktopLifetime.MainWindow = PresentationApi.Services!.GetRequiredService<MainWindow> ();
		}
		base.OnFrameworkInitializationCompleted ();
	}

}