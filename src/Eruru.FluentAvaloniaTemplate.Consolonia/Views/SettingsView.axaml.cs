using Avalonia.Controls;
using Eruru.FluentAvaloniaTemplate.Consolonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Presentation;

namespace Eruru.FluentAvaloniaTemplate.Consolonia.Views;

public partial class SettingsView : UserControl {

	public SettingsView () {
		DataContext = PresentationApi.Services!.GetRequiredService<SettingsViewModel> ();
		InitializeComponent ();
	}

}