using Avalonia.Controls;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Eruru.FluentAvaloniaTemplate;

public partial class SettingsPageView : UserControl {

	public SettingsPageView () {
		InitializeComponent ();
		DataContext = App.ServiceProvider?.GetRequiredService<SettingsPageViewModel> ();
	}

}