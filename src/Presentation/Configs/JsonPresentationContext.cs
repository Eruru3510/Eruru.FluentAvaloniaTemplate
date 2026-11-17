using System.Text.Json.Serialization;

namespace Presentation.Configs;

[JsonSourceGenerationOptions (UseStringEnumConverter = true)]
[JsonSerializable (typeof (PresentationConfig))]
public partial class JsonPresentationContext : JsonSerializerContext {

}