using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using FluentAvalonia.UI.Controls;

namespace Eruru.FluentAvaloniaTemplate.ViewModels;

public partial class NavigationItemViewModel (
	string content, Type viewType, Func<object?, object?>? getDataContext, object? state = null,
	FAIconSource? iconSource = null
) : ViewModelBase {

	[ObservableProperty]
	public partial string Content { get; set; } = content;
	[ObservableProperty]
	public partial FAIconSource? IconSource { get; set; } = iconSource;
	[ObservableProperty]
	public partial object? DataContext { get; set; }
	[ObservableProperty]
	public partial ObservableCollection<NavigationItemViewModel> Items { get; set; } = [];
	public Type ViewType { get; } = viewType;
	public NavigationItemViewModel? Parent { get; set; }

	readonly object? State = state;
	readonly Func<object?, object?>? GetDataContext = getDataContext;

	public void Initialize () {
		DataContext = GetDataContext?.Invoke (State);
	}

	public NavigationItemViewModel GetRoot () {
		return Parent == null ? this : Parent.GetRoot ();
	}

	public bool Contains (NavigationItemViewModel item) {
		foreach (var current in Items) {
			if (current == item || current.Contains (item)) {
				return true;
			}
		}
		return false;
	}

}