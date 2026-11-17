using Avalonia;
using Eruru.FluentAvaloniaTemplate.Dialogs;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using Eruru.FluentAvaloniaTemplate.Views;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Dialogs;

namespace Eruru.FluentAvaloniaTemplate;

public static class DependencyInjection {

	public static IServiceCollection AddHost (this IServiceCollection services) {
		services.AddSingleton<MainWindow> ();
		services.AddSingleton<MainWindowViewModel> ();
		services.AddSingleton<MainView> ();
		services.AddSingleton<MainViewModel> ();
		services.AddSingleton<HomeView> ();
		services.AddSingleton<HomeViewModel> ();
		services.AddSingleton<SubHomeView> ();
		services.AddSingleton<DynamicHomeView> ();
		services.AddSingleton<SettingsView> ();
		services.AddSingleton<SettingsViewModel> ();
		services.AddSingleton<AppViewModel> ();
		services.AddSingleton<IDialogService, DialogService> ();
		services.AddSingleton<Visual> (static x => x.GetRequiredService<MainView> ());
		return services;
	}

}