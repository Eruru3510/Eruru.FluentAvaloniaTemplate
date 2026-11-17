using System.Text.Encodings.Web;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection;
using Presentation.Configs;
using Presentation.Mapper;
using Presentation.Navigations;

namespace Presentation;

public static class DependencyInjection {

	public static IServiceCollection AddPresentation (this IServiceCollection services) {
		services.AddSingleton<NavigationService> ();
		services.AddSingleton (static x => TopLevel.GetTopLevel (x.GetRequiredService<Visual> ())!);
		services.AddSingleton (static x => x.GetRequiredService<TopLevel> ().Launcher);
		services.AddKeyedSingleton (ConfigIds.Config, static (_, _) => {
			var jsonSerializerOptions = new JsonSerializerOptions () {
				Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
				WriteIndented = true
			};
			jsonSerializerOptions.Converters.Add (new DateTimeJsonConverter ());
			return jsonSerializerOptions;
		});
		services.AddSingleton (static x => new JsonPresentationContext (
			x.GetRequiredKeyedService<JsonSerializerOptions> (ConfigIds.Config)
		));
		services.AddSingleton (static x => new JsonConfig<PresentationConfig> (
			x.GetRequiredKeyedService<IJsonConfigSource> (ConfigIds.Presentation),
			x.GetRequiredService<JsonPresentationContext> ().PresentationConfig
		));
		services.AddSingleton<PresentationMapper> ();
		return services;
	}

}