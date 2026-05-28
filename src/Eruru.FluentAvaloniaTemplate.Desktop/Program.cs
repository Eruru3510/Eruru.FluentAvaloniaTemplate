using Avalonia;
using Eruru.FluentAvaloniaTemplate.Resources;
using Eruru.FluentAvaloniaTemplate.Services;

namespace Eruru.FluentAvaloniaTemplate.Desktop {

	internal sealed class Program {

		// Initialization code. Don't use any Avalonia, third-party APIs or any
		// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
		// yet and stuff might break.
		[STAThread]
		public static void Main (string[] args) {
			using var singleInstanceService = new SingleInstanceService ($"{nameof (Eruru)}.{nameof (FluentAvaloniaTemplate)}");
			singleInstanceService.OnReceivedAsync += App.SingleInstanceServiceOnReceivedAsync;
			if (singleInstanceService.IsExists ()) {
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable VSTHRD002 // Avoid problematic synchronous waits
				singleInstanceService.SendAsync (LangKeys.Show).GetAwaiter ().GetResult ();
#pragma warning restore VSTHRD002 // Avoid problematic synchronous waits
#pragma warning restore IDE0079 // 请删除不必要的忽略
				return;
			}
			BuildAvaloniaApp ()
				.StartWithClassicDesktopLifetime (args);
		}

		// Avalonia configuration, don't remove; also used by visual designer.
		public static AppBuilder BuildAvaloniaApp () {
			return AppBuilder.Configure<App> ()
				.UsePlatformDetect ()
#if DEBUG
				.WithDeveloperTools ()
#endif
				.LogToTrace ()
				.AfterPlatformServicesSetup (static appBuilder => {
					appBuilder.ConfigureServices (static serviceCollection => {
						serviceCollection.AddCommonServices ();
						serviceCollection.AddViews ();
						serviceCollection.AddViewModels ();
						serviceCollection.AddJsonConfig ();
					});
				});
		}

	}

}