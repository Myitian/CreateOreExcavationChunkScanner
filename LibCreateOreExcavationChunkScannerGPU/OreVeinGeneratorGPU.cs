using ILGPU;
using ILGPU.Runtime;
using LibCreateOreExcavationChunkScanner.Random;
using System.Runtime.InteropServices;

namespace LibCreateOreExcavationChunkScanner.GPU;

public class OreVeinGeneratorGPU
{
    public static Dictionary<ChunkPosition, OreData> ParallelGetData(
        Accelerator accelerator,
        AcceleratorStream stream,
        long seed,
        ChunkPosition startChunk,
        ChunkPosition endChunk)
    {
        int lengthX = checked(Math.Abs(endChunk.X - startChunk.X + 1));
        int lengthZ = checked(Math.Abs(endChunk.Z - startChunk.Z + 1));
        ArgumentOutOfRangeException.ThrowIfGreaterThan((long)lengthX * lengthZ, Array.MaxLength);
        int startX = Math.Min(startChunk.X, endChunk.X);
        int startZ = Math.Min(startChunk.Z, endChunk.Z);
        OreData[] resultArray;

        ReadOnlySpan<VeinRecipe> recipes = CollectionsMarshal.AsSpan(OreVeinGenerator.Recipes);
        Span<RandomSpreadStructurePlacement> placements = stackalloc RandomSpreadStructurePlacement[recipes.Length];
        for (int i = 0; i < recipes.Length; i++)
            placements[i] = recipes[i].Placement;

        using (var recipeBuffer = accelerator.Allocate1D<RandomSpreadStructurePlacement>(placements.Length))
        {
            ArrayView<RandomSpreadStructurePlacement> view = recipeBuffer.View.BaseView;
            view.CopyFromCPU(stream, (ReadOnlySpan<RandomSpreadStructurePlacement>)placements);

            using var resultBuffer = accelerator.Allocate2DDenseX<OreData>(new(lengthX, lengthZ));

            var kernel = accelerator.LoadAutoGroupedKernel<Index2D, Index2D, long, ArrayView<RandomSpreadStructurePlacement>, ArrayView2D<OreData, Stride2D.DenseX>>(Kernel);
            kernel(stream, resultBuffer.IntExtent, new(startX, startZ), seed, view, resultBuffer.View);

            resultArray = GC.AllocateUninitializedArray<OreData>(checked(lengthX * lengthZ));
            resultBuffer.View.BaseView.CopyToCPU(stream, resultArray);

            stream.Synchronize();
        }

        Dictionary<ChunkPosition, OreData> result = [];
        for (int z = 0, offset = 0; z < lengthZ; z++, offset += lengthX)
        {
            for (int x = 0; x < lengthX; x++)
            {
                ref OreData data = ref resultArray[offset + x];
                if (data.ID < 0)
                    continue;
                result.Add(new(startX + x, startZ + z), data);
            }
        }
        return result;
    }
    public static void Kernel(
        Index2D chunkPos,
        Index2D offset,
        long seed,
        ArrayView<RandomSpreadStructurePlacement> veinRecipes,
        ArrayView2D<OreData, Stride2D.DenseX> result)
    {
        OreData data = new();
        int chunkX = chunkPos.X + offset.X;
        int chunkZ = chunkPos.Y + offset.Y;
        int id = -1;
        for (int i = 0; i < veinRecipes.IntLength; i++)
        {
            ChunkPosition chunkPosResult = veinRecipes[i].GetStartChunk(seed, chunkX, chunkZ);
            if (chunkPosResult.X == chunkX && chunkPosResult.Z == chunkZ)
            {
                id = i;
                break;
            }
        }
        data.ID = id;
        if (id >= 0)
            data.RandomMul = new LegacyRandomStruct(seed ^ ((long)chunkX << 32 | (uint)chunkZ)).NextSingle();
        result[chunkPos] = data;
    }
}