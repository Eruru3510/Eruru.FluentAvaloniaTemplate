namespace Eruru.FluentAvaloniaTemplate.Services;

public class PathService {

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