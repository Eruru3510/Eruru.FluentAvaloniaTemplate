using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Eruru.FluentAvaloniaTemplate.ViewModels;

public partial class NavigationViewModel : ViewModelBase {

	[ObservableProperty]
	public partial ObservableCollection<NavigationItemViewModel> Items { get; set; } = [];
	[ObservableProperty]
	public partial ObservableCollection<NavigationItemViewModel> FooterItems { get; set; } = [];

}