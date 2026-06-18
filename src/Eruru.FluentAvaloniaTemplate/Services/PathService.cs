namespace Eruru.FluentAvaloniaTemplate.Services;

#pragma warning disable CA1052 // 静态容器类型应为 Static 或 NotInheritable
public class PathService {
#pragma warning restore CA1052 // 静态容器类型应为 Static 或 NotInheritable

	public static string GetSingleInstanceLockPath (string name) {
		return Path.Combine (Path.GetTempPath (), nameof (Eruru), Api.AppId, $"{name}.lock");
	}

	public string GetAppDirectory () {
		return Path.Combine (Environment.GetFolderPath (Environment.SpecialFolder.Personal), nameof (Eruru), Api.AppId);
	}

	public string GetConfigPath () {
		return Path.Combine (GetAppDirectory (), "Config.json");
	}

}