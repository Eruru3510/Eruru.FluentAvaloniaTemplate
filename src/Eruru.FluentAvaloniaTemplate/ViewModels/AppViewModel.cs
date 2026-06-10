using CommunityToolkit.Mvvm.ComponentModel;

namespace Eruru.FluentAvaloniaTemplate.ViewModels;

public partial class AppViewModel : ViewModelBase {

	[ObservableProperty]
	public partial bool IsShowTrayIcon { get; set; } = true;

}