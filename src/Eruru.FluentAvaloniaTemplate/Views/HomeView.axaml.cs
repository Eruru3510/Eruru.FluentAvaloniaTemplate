using Avalonia.Controls;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Presentation;

namespace Eruru.FluentAvaloniaTemplate.Views;

public partial class HomeView : UserControl {

	public HomeView () {
		DataContext = PresentationApi.Services!.GetRequiredService<HomeViewModel> ();
		InitializeComponent ();
	}

}