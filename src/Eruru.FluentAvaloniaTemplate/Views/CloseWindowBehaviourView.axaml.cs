using Avalonia.Controls;
using Eruru.FluentAvaloniaTemplate.ViewModels;

namespace Eruru.FluentAvaloniaTemplate.Views;

public partial class CloseWindowBehaviourView : UserControl {

	public CloseWindowBehaviourView () {
		DataContext = new CloseWindowBehaviourViewModel ();
		InitializeComponent ();
	}

}