using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Media;

namespace Eruru.FluentAvaloniaTemplate.JsonConverters {

	public class ColorJsonConverter : JsonConverter<Color> {

		public override Color Read (ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) {
			var index = reader.TokenStartIndex;
			if (!JsonDocument.TryParseValue (ref reader, out var jsonDocument)) {
				goto Exception;
			}
			try {
				if (!jsonDocument.RootElement.TryGetProperty ("r", out var value) || !value.TryGetByte (out var r)) {
					goto Exception;
				}
				if (!jsonDocument.RootElement.TryGetProperty ("g", out value) || !value.TryGetByte (out var g)) {
					goto Exception;
				}
				if (!jsonDocument.RootElement.TryGetProperty ("b", out value) || !value.TryGetByte (out var b)) {
					goto Exception;
				}
				if (!jsonDocument.RootElement.TryGetProperty ("a", out value) || !value.TryGetByte (out var a)) {
					goto Exception;
				}
				return new Color (a, r, g, b);
			} finally {
				jsonDocument.Dispose ();
			}
		Exception:
			throw new JsonException (
				$"The expectation was a JSON object, {{\"r\":0,\"g\":0,\"b\":0,\"a\":0}}. {nameof (reader.TokenStartIndex)}: {index}"
			);
		}

		public override void Write (Utf8JsonWriter writer, Color value, JsonSerializerOptions options) {
			ArgumentNullException.ThrowIfNull (writer, nameof (writer));
			writer.WriteStartObject ();
			writer.WriteNumber ("r", value.R);
			writer.WriteNumber ("g", value.G);
			writer.WriteNumber ("b", value.B);
			writer.WriteNumber ("a", value.A);
			writer.WriteEndObject ();
		}

	}

}