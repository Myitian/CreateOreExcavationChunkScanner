using LibCreateOreExcavationChunkScanner.Random;

namespace LibCreateOreExcavationChunkScanner;

public class OreVeinGenerator
{
    public static readonly List<VeinRecipe> Recipes = [];
    public static void SortRecipes()
    {
        Recipes.Sort((a, b) =>
        {
            int result = a.Priority.CompareTo(b.Priority);
            if (result != 0)
                return result;
            return string.Compare(a.ID, b.ID);
        });
    }
    public static VeinRecipe? GetVeinRecipeByIndex(int index)
    {
        if (index < 0 || index > Recipes.Count)
            return null;
        return Recipes[index];
    }

    public static LegacyRandomStruct RNGFromChunk(in Chunk chunk)
    {
        return new(chunk.Seed ^ (((long)chunk.Pos.X << 32) | (uint)chunk.Pos.Z));
    }
    public static OreData GetData(in Chunk chunk)
    {
        OreData data = new();
        int recipe = Pick(in chunk);
        data.ID = recipe;
        if (recipe >= 0)
            data.RandomMul = RNGFromChunk(in chunk).NextSingle();
        return data;
    }
    public static int Pick(in Chunk chunk)
    {
        // ***** Skip biome check *****
        int x = chunk.Pos.X;
        int z = chunk.Pos.Z;
        for (int i = 0; i < Recipes.Count; i++)
        {
            VeinRecipe recipe = Recipes[i];
            ChunkPosition chunkPos = recipe.Placement.GetStartChunk(chunk.Seed, x, z);
            if (chunkPos == chunk.Pos)
                return i;
        }
        return -1;
    }
}