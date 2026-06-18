namespace Eruru.FluentAvaloniaTemplate.ViewModels;

public partial class AskMinimizeToTrayIconViewModel : ViewModelBase {

	public MinimizeToTrayIconType MinimizeToTrayIconType { get; set; } = MinimizeToTrayIconType.MinimizeToTrayIcon;
	public bool IsDoNotAskAgain { get; set; }

}