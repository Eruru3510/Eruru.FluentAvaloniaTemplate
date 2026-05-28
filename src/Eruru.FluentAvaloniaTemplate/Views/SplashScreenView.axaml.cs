using Avalonia.Controls;
using Eruru.FluentAvaloniaTemplate.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Eruru.FluentAvaloniaTemplate.Views {

	public partial class SplashScreenView : UserControl {

		public SplashScreenView () {
			InitializeComponent ();
			DataContext = App.ServiceProvider?.GetRequiredService<SplashScreenViewModel> ();
		}

	}

}