using System.Collections.ObjectModel;

namespace Eruru.FluentAvaloniaTemplate {

	public static class Api {

		public static Collection<KeyValuePair<string, string>> AppLanguages { get; } = [
			new ("English (United States)", "en-US"),
			new ("中文 (简体)", "zh-CN")
		];

	}

}