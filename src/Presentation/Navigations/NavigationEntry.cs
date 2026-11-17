namespace Presentation.Navigations;

public class NavigationEntry (
	string id, string title, Type viewType,
	string? parentId = null, Func<object>? onGetDataContext = null,
	bool isShowBreadcrumb = true, bool isShowInMenu = true, object? iconSource = null,
	bool isFooterMenuItem = false
) {

	public string Id { get; } = id;
	public string Title { get; } = title;
	public Type ViewType { get; } = viewType;
	public string? ParentId { get; } = parentId;
	public Func<object>? OnGetDataContext { get; } = onGetDataContext;
	public bool IsShowBreadcrumb { get; } = isShowBreadcrumb;
	public bool IsShowInMenu { get; } = isShowInMenu;
	public object? IconSource { get; } = iconSource;
	public bool IsFooterMenuItem { get; } = isFooterMenuItem;

	public override string ToString () {
		return Title;
	}

}