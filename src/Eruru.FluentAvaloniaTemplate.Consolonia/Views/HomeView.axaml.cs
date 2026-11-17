using Avalonia.Controls;
using Eruru.FluentAvaloniaTemplate.Consolonia.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Presentation;

namespace Eruru.FluentAvaloniaTemplate.Consolonia.Views;

public partial class HomeView : UserControl {

	public HomeView () {
		DataContext = PresentationApi.Services!.GetRequiredService<HomeViewModel> ();
		InitializeComponent ();
	}

}