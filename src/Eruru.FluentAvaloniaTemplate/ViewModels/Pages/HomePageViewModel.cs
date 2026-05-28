using CommunityToolkit.Mvvm.ComponentModel;

namespace Eruru.FluentAvaloniaTemplate.ViewModels {

	public partial class HomePageViewModel : ViewModelBase {

		[ObservableProperty]
		public partial string Text { get; set; } = string.Empty;

	}

}