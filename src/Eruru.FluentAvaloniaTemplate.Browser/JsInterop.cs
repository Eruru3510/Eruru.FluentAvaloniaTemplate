using System.Runtime.InteropServices.JavaScript;

namespace Eruru.FluentAvaloniaTemplate.Browser {

	internal static partial class JsInterop {

		[JSImport ("globalThis.window.localStorage.setItem")]
		public static partial void LocalStorageSetItem (string key, string value);

		[JSImport ("globalThis.window.localStorage.getItem")]
		public static partial string LocalStorageGetItem (string key);

		[JSImport ("globalThis.window.console.error")]
		public static partial void ConsoleError (string text);

		[JSImport ("INTERNAL.loadSatelliteAssemblies")]
		public static partial Task LoadSatelliteAssembliesAsync (string[] culturesToLoad);

	}

}