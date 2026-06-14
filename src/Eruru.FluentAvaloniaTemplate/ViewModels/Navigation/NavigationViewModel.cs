using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Eruru.FluentAvaloniaTemplate.ViewModels;

public partial class NavigationViewModel : ViewModelBase {

	[ObservableProperty]
	public partial ObservableCollection<NavigationItemViewModel> Items { get; set; } = [];
	[ObservableProperty]
	public partial ObservableCollection<NavigationItemViewModel> FooterItems { get; set; } = [];

	public bool Contains (NavigationItemViewModel item) {
		foreach (var current in Items) {
			if (current == item || current.Contains (item)) {
				return true;
			}
		}
		foreach (var current in FooterItems) {
			if (current == item || current.Contains (item)) {
				return true;
			}
		}
		return false;
	}

}