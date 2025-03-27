using LibCreateOreExcavationChunkScanner;
using System.Text.Json.Serialization;

namespace CreateOreExcavationChunkScanner;

[JsonSourceGenerationOptions(
    AllowOutOfOrderMetadataProperties = true,
    IncludeFields = true)]
[JsonSerializable(typeof(BaseRecipe))]
internal partial class SourceGenerationContext : JsonSerializerContext
{
}