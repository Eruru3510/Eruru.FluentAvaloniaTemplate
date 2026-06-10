using Avalonia.Controls;
using Avalonia.Interactivity;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Eruru.FluentAvaloniaTemplate;

public partial class SettingsPageView : UserControl {

	public SettingsPageView () {
		InitializeComponent ();
		DataContext = App.ServiceProvider?.GetRequiredService<SettingsPageViewModel> ();
	}

	void FASettingsExpanderItem_Click (object? sender, RoutedEventArgs e) {
		_ = TopLevel.GetTopLevel (this)?.Launcher.LaunchUriAsync (
			new ("https://github.com/amwx/FluentAvalonia")
		).ContinueWithShowExceptionAsync ();
	}

}