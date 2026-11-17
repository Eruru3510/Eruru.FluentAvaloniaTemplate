using Avalonia.Controls;
using Eruru.FluentAvaloniaTemplate.Consolonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Presentation;

namespace Eruru.FluentAvaloniaTemplate.Consolonia.Views;

public partial class MainWindow : Window {

	public MainWindow () {
		DataContext = PresentationApi.Services!.GetRequiredService<MainWindowViewModel> ();
		InitializeComponent ();
		Content = PresentationApi.Services!.GetRequiredService<MainView> ();
	}

}