namespace Eruru.FluentAvaloniaTemplate.iOS {

#pragma warning disable CA1515 // 考虑将公共类型设为内部类型
#pragma warning disable CA1724 // 类型名与命名空间名称整体或部分冲突
	public class Application {
#pragma warning restore CA1724 // 类型名与命名空间名称整体或部分冲突
#pragma warning restore CA1515 // 考虑将公共类型设为内部类型

		// This is the main entry point of the application.
		static void Main (string[] args) {
			// if you want to use a different Application Delegate class from "AppDelegate"
			// you can specify it here.
			UIApplication.Main (args, null, typeof (AppDelegate));
		}

	}

}