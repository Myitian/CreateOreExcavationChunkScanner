namespace LibCreateOreExcavationChunkScanner;

public readonly struct Chunk(long seed, int x, int z)
{
    public readonly long Seed = seed;
    public readonly ChunkPosition Pos = new(x, z);
}