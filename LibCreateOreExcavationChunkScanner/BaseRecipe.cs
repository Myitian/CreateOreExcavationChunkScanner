using System.Text.Json.Serialization;

namespace LibCreateOreExcavationChunkScanner;

[JsonPolymorphic(
    IgnoreUnrecognizedTypeDiscriminators = true,
    TypeDiscriminatorPropertyName = "type")]
[JsonDerivedType(
    typeof(VeinRecipe),
    typeDiscriminator: "createoreexcavation:vein")]
public class BaseRecipe
{
}