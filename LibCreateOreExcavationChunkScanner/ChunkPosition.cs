namespace LibCreateOreExcavationChunkScanner;

public struct ChunkPosition(int x, int z) : IEquatable<ChunkPosition>
{
    public int X = x, Z = z;

    public readonly bool Equals(ChunkPosition other)
        => X == other.X && Z == other.Z;
    public override readonly bool Equals(object? obj)
        => obj is ChunkPosition pos && Equals(pos);
    public override readonly int GetHashCode()
        => (X << 16) ^ Z;
    public override readonly string ToString()
        => $"({X}, {Z})";
    public static bool operator ==(ChunkPosition left, ChunkPosition right)
        => left.Equals(right);
    public static bool operator !=(ChunkPosition left, ChunkPosition right)
        => !left.Equals(right);
}