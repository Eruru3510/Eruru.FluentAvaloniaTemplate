namespace Presentation.Navigations;

public class NavigationItemHistory (string id, object? dataContext) {

	public string Id { get; } = id;
	public object? DataContext { get; } = dataContext;

};