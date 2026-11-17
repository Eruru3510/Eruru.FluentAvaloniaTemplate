using Avalonia;
using Eruru.FluentAvaloniaTemplate.Consolonia.Dialogs;
using Eruru.FluentAvaloniaTemplate.Consolonia.ViewModels;
using Eruru.FluentAvaloniaTemplate.Consolonia.Views;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Dialogs;

namespace Eruru.FluentAvaloniaTemplate;

public static class DependencyInjection {

	public static IServiceCollection AddHost (this IServiceCollection services) {
		services.AddSingleton<MainWindow> ();
		services.AddSingleton<MainWindowViewModel> ();
		services.AddSingleton<MainView> ();
		services.AddSingleton<MainViewModel> ();
		services.AddSingleton<SettingsView> ();
		services.AddSingleton<SettingsViewModel> ();
		services.AddSingleton<HomeView> ();
		services.AddSingleton<HomeViewModel> ();
		services.AddSingleton<IDialogService, DialogService> ();
		services.AddSingleton<Visual> (static x => x.GetRequiredService<MainWindow> ());
		return services;
	}

}