namespace Eruru.FluentAvaloniaTemplate.Services;

#pragma warning disable CA1052 // 静态容器类型应为 Static 或 NotInheritable
public class PathService {
#pragma warning restore CA1052 // 静态容器类型应为 Static 或 NotInheritable

	public static string ConfigPath { get; } = Path.Combine (
		Environment.GetFolderPath (Environment.SpecialFolder.Personal),
		nameof (Eruru), nameof (FluentAvaloniaTemplate), "Config.json"
	);

}