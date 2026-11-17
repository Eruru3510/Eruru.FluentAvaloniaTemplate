using CommunityToolkit.Mvvm.ComponentModel;
using Presentation.Configs;

namespace Eruru.FluentAvaloniaTemplate.ViewModels;

public partial class CloseWindowBehaviourViewModel : ViewModelBase {

	[ObservableProperty]
	public partial CloseWindowBehaviour CloseWindowBehaviour { get; set; } = CloseWindowBehaviour.MinimizeToTray;
	[ObservableProperty]
	public partial bool IsDoNotAskAgain { get; set; }

}