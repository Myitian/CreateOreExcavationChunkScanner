namespace LibCreateOreExcavationChunkScanner.Random;

public struct LegacyRandomStruct
{
    private long seed;
    public LegacyRandomStruct(long seed)
    {
        SetSeed(seed);
    }
    public void SetSeed(long seed)
    {
        this.seed = (seed ^ 25214903917) & 0xFFFFFFFFFFFF;
    }
    public int NextBits(int bits)
    {
        seed = seed * 25214903917 + 11 & 0xFFFFFFFFFFFF;
        return (int)(seed >> 48 - bits);
    }
    public float NextSingle()
    {
        return NextBits(24) * 5.9604645E-8f;
    }
}