using Avalonia.Controls;
using FluentAvalonia.UI.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace Eruru.FluentAvaloniaTemplate;

public class NavigationService : IFANavigationPageFactory {

	public FAFrame? Frame { get; set; }

	public Control GetPage (Type srcType) {
		return GetPageFromObject (App.ServiceProvider?.GetRequiredService (srcType));
	}

	public Control GetPageFromObject (object? target) {
		return target as Control ?? new TextBlock () { Text = $"{target}" };
	}

}