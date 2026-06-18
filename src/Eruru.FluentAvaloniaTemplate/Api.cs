using System.Collections.ObjectModel;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;

namespace Eruru.FluentAvaloniaTemplate;

public static class Api {

	public static string AppId { get; } = nameof (FluentAvaloniaTemplate);
	public static Collection<KeyValuePair<string, string>> AppLanguages { get; } = [
		new ("English (United States)", "en-US"),
		new ("中文 (简体)", "zh-CN")
	];

	public static async Task ShutdownAsync () {
		if (App.Current?.ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop) {
			return;
		}
		await Dispatcher.UIThread.InvokeAsync (() => desktop.Shutdown ());
	}

}