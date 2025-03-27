namespace LibCreateOreExcavationChunkScanner.Random;

public struct ChunkRandomStruct(long seed)
{
    private LegacyRandomStruct baseRandom = new(seed);
    public int NextBits(int bits)
    {
        return baseRandom.NextBits(bits);
    }
    public int Next(int bound)
    {
        if ((bound & bound - 1) == 0)
            return (int)((long)bound * NextBits(31) >> 31);
        int i;
        int j;
        do
        {
            i = NextBits(31);
            j = i % bound;
        } while (i - j + (bound - 1) < 0);
        return j;
    }
    public void SetRegionSeed(long worldSeed, int regionX, int regionZ, int salt)
    {
        baseRandom.SetSeed(regionX * 341873128712 + regionZ * 132897987541 + worldSeed + salt);
    }
}