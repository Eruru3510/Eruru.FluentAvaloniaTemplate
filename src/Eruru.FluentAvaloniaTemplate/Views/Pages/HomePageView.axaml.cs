using Avalonia.Controls;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Eruru.FluentAvaloniaTemplate;

public partial class HomePageView : UserControl {

	public HomePageView () {
		InitializeComponent ();
		DataContext = App.ServiceProvider?.GetRequiredService<HomePageViewModel> ();
	}

}