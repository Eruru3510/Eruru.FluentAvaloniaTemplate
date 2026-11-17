using Avalonia;
using Avalonia.Threading;
using Eruru.FluentAvaloniaTemplate.Views;
using Microsoft.Extensions.DependencyInjection;
using Presentation;
using Presentation.Configs;
using Presentation.Dialogs;

namespace Eruru.FluentAvaloniaTemplate.Desktop;

sealed class Program {

	static readonly SingleInstanceSystem SingleInstanceService = new (nameof (Eruru.FluentAvaloniaTemplate));

	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main (string[] args) {
		try {
			var b = Thread.CurrentThread.GetApartmentState ();
			if (SingleInstanceService.IsExists ()) {
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
				SingleInstanceService.SendAsync (CommandIds.ShowWindow).GetAwaiter ().GetResult ();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
				return;
			}
			_ = SingleInstanceService.StartServerAsync (static async (message, cancellationToken) => {
				switch (message) {
					default:
						await PresentationApi.Services!.GetRequiredService<IDialogService> ().ShowMessageAsync (
							$"未知消息 {message}", cancellationToken: cancellationToken
						).ConfigureAwait (true);
						break;
					case CommandIds.ShowWindow:
						await Dispatcher.UIThread.InvokeAsync (
							static () => PresentationApi.Services!.GetRequiredService<MainWindow> ().RestoreWindow ()
						).GetTask ().ConfigureAwait (true);
						break;
				}
			}).OnFaultedWriteLineAsync ();
			var a = Thread.CurrentThread.GetApartmentState ();
			BuildAvaloniaApp ()
				.StartWithClassicDesktopLifetime (args);
		} finally {
			SingleInstanceService.Dispose ();
		}
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp () {
		return AppBuilder.Configure<App> ()
			.UsePlatformDetect ()
			.LogToTrace ()
			.AfterSetup (static _ => {
				var services = new ServiceCollection ();
				services.AddHost ();
				services.AddPresentation ();
				services.AddKeyedSingleton<IJsonConfigSource> (
					ConfigIds.Presentation, static (_, _) => new JsonConfigFileSource (Path.Combine (
						Environment.GetFolderPath (Environment.SpecialFolder.LocalApplicationData),
						"Eruru", "FluentAvaloniaTemplate", "PresentationConfig.json"
					))
				);
				PresentationApi.Services = services.BuildServiceProvider ();
			});
	}

}