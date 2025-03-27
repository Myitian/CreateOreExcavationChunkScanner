using System.Text.Json.Serialization;

namespace LibCreateOreExcavationChunkScanner;

public class VeinRecipe : BaseRecipe
{
    [JsonIgnore]
    public string? ID;
    [JsonPropertyName("priority")]
    public int Priority;
    [JsonPropertyName("name")]
    public string? VeinName;
    [JsonPropertyName("biomeWhitelist")]
    public string? BiomeWhitelist;
    [JsonPropertyName("biomeBlacklist")]
    public string? BiomeBlacklist;
    [JsonPropertyName("finite")]
    public bool? Finite;
    [JsonPropertyName("amountMin")]
    public float AmountMultiplierMin;
    [JsonPropertyName("amountMax")]
    public float AmountMultiplierMax;
    [JsonPropertyName("placement")]
    public RandomSpreadStructurePlacement Placement;
}