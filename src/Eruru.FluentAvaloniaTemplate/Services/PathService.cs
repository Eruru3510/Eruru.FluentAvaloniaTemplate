namespace Eruru.FluentAvaloniaTemplate.Services;

public class PathService {

	public static string ConfigPath { get; } = Path.Combine (
		Environment.GetFolderPath (Environment.SpecialFolder.Personal),
		nameof (Eruru), nameof (FluentAvaloniaTemplate), "Config.json"
	);

}