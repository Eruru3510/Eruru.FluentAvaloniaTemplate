using System.Text.Json.Serialization;
using Eruru.FluentAvaloniaTemplate.Models;

namespace Eruru.FluentAvaloniaTemplate;

[JsonSourceGenerationOptions (UseStringEnumConverter = true)]
[JsonSerializable (typeof (Config))]
public partial class JsonConfigContext : JsonSerializerContext {

}