using Avalonia.Media;
using Eruru.FluentAvaloniaTemplate.Views;
using FluentAvalonia.UI.Windowing;

namespace Eruru.FluentAvaloniaTemplate;

public class SplashScreen (SplashScreenView splashScreenView) : IFAApplicationSplashScreen {

	public string? AppName { get; }
	public IImage? AppIcon { get; }
	public object? SplashScreenContent { get; } = splashScreenView;
	public int MinimumShowTime { get; }
#if DEBUG
		= 1000;
#endif

	public Task RunTasks (CancellationToken cancellationToken) {
		return Task.CompletedTask;
	}

}