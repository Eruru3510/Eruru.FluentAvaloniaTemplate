using System.Collections.ObjectModel;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Mapper;

namespace Presentation.Navigations;

public partial class NavigationService : ObservableObject {

	[ObservableProperty]
	public partial ObservableCollection<NavigationItem> MenuItems { get; set; } = [];
	[ObservableProperty]
	public partial ObservableCollection<NavigationItem> FooterMenuItems { get; set; } = [];
	[ObservableProperty]
	public partial ObservableCollection<NavigationEntry> BreadcrumbItems { get; set; } = [];
	[ObservableProperty]
	public partial object? CurrentView { get; private set; }
	[ObservableProperty]
	public partial NavigationItem? CurrentItem { get; private set; }
	public bool CanBack => Histories.Count > 0;

	NavigationEntry? CurrentEntry;
	readonly List<NavigationItemHistory> Histories = [];
	readonly Dictionary<string, NavigationEntry> Entries = [];

	public void Register (NavigationEntry navigationEntry) {
		ArgumentNullException.ThrowIfNull (navigationEntry);
		Entries.Add (navigationEntry.Id, navigationEntry);
		if (navigationEntry.IsShowInMenu) {
			var menuItems = navigationEntry.IsFooterMenuItem ? FooterMenuItems : MenuItems;
			var item = PresentationApi.Services!.GetRequiredService<PresentationMapper> ().ToNavigationItem (navigationEntry);
			if (navigationEntry.ParentId == null) {
				menuItems.Add (item);
			} else {
				var parentItem = Find (navigationEntry.ParentId, menuItems)!;
				parentItem.SubItems.Add (item);
				item.Parent = parentItem;
			}
		}
	}

	public void Navigate (string id, object? dataContext = null, bool recordHistory = true) {
		var oldEntry = CurrentEntry;
		var entry = Entries[id];
		if (oldEntry != null && oldEntry.Id == entry.Id) {
			return;
		}
		CurrentEntry = entry;
		var oldItem = CurrentItem;
		var item = GetOrParent (entry.Id, entry.IsFooterMenuItem);
		CurrentItem = item;
		var oldView = CurrentView as Visual;
		var view = (Visual)PresentationApi.Services!.GetRequiredService (entry.ViewType);
		view.DataContext = dataContext ?? entry.OnGetDataContext?.Invoke () ?? view.DataContext;
		CurrentView = view;
		if (oldEntry != null && recordHistory) {
			Histories.Add (new (oldEntry.Id, oldView!.DataContext));
		}
		SetBreadcrumb (entry);
		OnPropertyChanged (nameof (CanBack));
	}

	public void Back () {
		var oldHistory = Histories[^1];
		Histories.RemoveAt (Histories.Count - 1);
		Navigate (oldHistory.Id, oldHistory.DataContext, false);
	}

	NavigationItem? GetOrParent (string id, bool isFooterMenuItem) {
		return GetOrParent (Find (id, isFooterMenuItem ? FooterMenuItems : MenuItems));
	}
	NavigationItem? GetOrParent (NavigationItem? item) {
		if (item == null) {
			return null;
		}
		if (item.IsShowInMenu) {
			return item;
		}
		return item.Parent == null ? null : GetOrParent (item.Parent);
	}

	NavigationItem? Find (string id, IList<NavigationItem> items) {
		foreach (var item in items) {
			var subItem = Find (id, item.SubItems);
			if (subItem != null) {
				return subItem;
			}
			if (item.Id == id) {
				return item;
			}
		}
		return null;
	}

	void SetBreadcrumb (NavigationEntry entry) {
		BreadcrumbItems.Clear ();
		AppendBreadcrumb (entry, BreadcrumbItems);
	}
	void AppendBreadcrumb (NavigationEntry entry, IList<NavigationEntry> breadcrumbItems) {
		breadcrumbItems.Insert (0, entry);
		if (entry.ParentId != null) {
			AppendBreadcrumb (Entries[entry.ParentId], breadcrumbItems);
		}
	}

}