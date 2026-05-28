using Android.Runtime;
using Avalonia;
using Avalonia.Android;

namespace Eruru.FluentAvaloniaTemplate.Android {

	[Application]
#pragma warning disable CA1724 // 类型名与命名空间名称整体或部分冲突
	public class Application : AvaloniaAndroidApplication<App> {
#pragma warning restore CA1724 // 类型名与命名空间名称整体或部分冲突

		protected Application (nint javaReference, JniHandleOwnership transfer) : base (javaReference, transfer) {

		}

		protected override AppBuilder CustomizeAppBuilder (AppBuilder builder) {
			return base.CustomizeAppBuilder (builder)
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