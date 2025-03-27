using LibCreateOreExcavationChunkScanner.Random;
using System.Text.Json.Serialization;

namespace LibCreateOreExcavationChunkScanner;

public struct RandomSpreadStructurePlacement
{
    [JsonPropertyName("spacing")]
    public int Spacing;
    [JsonPropertyName("separation")]
    public int Separation;
    [JsonPropertyName("salt")]
    public int Salt;
    [JsonIgnore]
    public SpreadType SpreadType;

    [JsonPropertyName("spreadType")]
    public string? SpreadTypeString
    {
        readonly get => SpreadType switch
        {
            SpreadType.Triangular => "triangular",
            _ => "linear"
        };
        set => SpreadType = value switch
        {
            "triangular" => SpreadType.Triangular,
            _ => SpreadType.Linear
        };
    }

    public readonly ChunkPosition GetStartChunk(long seed, int chunkX, int chunkZ)
    {
        float spacing = Spacing;
        int k = (int)MathF.Floor(chunkX / spacing);
        int m = (int)MathF.Floor(chunkZ / spacing);
        ChunkRandomStruct rng = new(0);
        rng.SetRegionSeed(seed, k, m, Salt);
        int n = Spacing - Separation;
        int o = Get(ref rng, n);
        int p = Get(ref rng, n);
        return new(k * Spacing + o, m * Spacing + p);
    }

    private readonly int Get(ref ChunkRandomStruct random, int bound)
    {
        return SpreadType switch
        {
            SpreadType.Triangular => (random.Next(bound) + random.Next(bound)) / 2,
            _ => random.Next(bound)
        };
    }
}