using ILGPU;
using ILGPU.Runtime;
using LibCreateOreExcavationChunkScanner;
using LibCreateOreExcavationChunkScanner.GPU;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO.Compression;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CreateOreExcavationChunkScanner;

public partial class Program
{
    public static void Main()
    {
        while (true)
        {
            string path;
            if (OreVeinGenerator.Recipes.Count == 0)
            {
                Console.Error.WriteLine("Enter mod jar or datapack path:");
                path = Console.ReadLine().AsSpan().Trim().Trim('"').ToString();
            }
            else
            {
                Console.Error.WriteLine("Enter mod jar or datapack path (leave space to finish importing):");
                ReadOnlySpan<char> span = Console.ReadLine().AsSpan().Trim();
                if (span.IsEmpty)
                    break;
                path = span.Trim('"').ToString();
            }
            try
            {
                Regex regex = VeinRecipePath();
                if (File.Exists(path))
                {
                    using ZipArchive archive = ZipFile.OpenRead(path);
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        Match m = regex.Match(entry.FullName);
                        if (!m.Success)
                            continue;
                        using Stream rs = entry.Open();
                        LoadRecipe(m, rs);
                    }
                }
                else if (Directory.Exists(path))
                {
                    DirectoryInfo dir = new(path);
                    foreach (FileInfo file in dir.EnumerateFiles("*.json", new EnumerationOptions()
                    {
                        MatchCasing = MatchCasing.CaseInsensitive,
                        RecurseSubdirectories = true,
                        IgnoreInaccessible = true
                    }))
                    {
                        string relative = Path.GetRelativePath(dir.FullName, file.FullName);
                        Match m = regex.Match(relative);
                        if (!m.Success)
                            continue;
                        using FileStream rs = file.OpenRead();
                        LoadRecipe(m, rs);
                    }
                }
                else
                {
                    Console.WriteLine("File does not exist!");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return;
            }
        }
        OreVeinGenerator.SortRecipes();
        long seed = Read<long>("Enter world seed:", "Please enter a valid int64.");
        ChunkPosition startChunk = new()
        {
            X = Read<int>("Enter start chunk X (inclusive):", "Please enter a valid int32."),
            Z = Read<int>("Enter start chunk Z (inclusive):", "Please enter a valid int32.")
        };
        ChunkPosition endChunk = new()
        {
            X = Read<int>("Enter end chunk X (inclusive):", "Please enter a valid int32."),
            Z = Read<int>("Enter end chunk Z (inclusive):", "Please enter a valid int32.")
        };
        using Context ctx = Context.Create(b => b.Default().EnableAlgorithms());
        ImmutableArray<Device> devices = ctx.Devices;
        Console.Error.WriteLine("Select a device:");
        for (int i = 0; i < devices.Length; i++)
        {
            Device device = devices[i];
            Console.Error.WriteLine($"[{i}] {device.Name}");
        }
        int selected;
        Console.Error.WriteLine();
        Console.Error.WriteLine("Enter device index:");
        while (!int.TryParse(Console.ReadLine(), out selected))
            Console.WriteLine("Value is out of range!");
        using Accelerator accelerator = ctx.Devices[selected].CreateAccelerator(ctx);
        AcceleratorStream stream = accelerator.DefaultStream;

        Dictionary<ChunkPosition, OreData> veins = OreVeinGeneratorGPU.ParallelGetData(accelerator, stream, seed, startChunk, endChunk);

        Console.Error.WriteLine();
        Console.Error.WriteLine($"Found vein: {veins.Count}");
        Console.Out.WriteLine("ChunkX,ChunkZ,ID,Amount");
        foreach ((ChunkPosition pos, OreData data) in veins)
        {
            VeinRecipe? vein = OreVeinGenerator.GetVeinRecipeByIndex(data.ID);
            Debug.Assert(vein is not null);
            Console.Out.WriteLine($"{pos.X},{pos.Z},{vein.ID},{float.Lerp(vein.AmountMultiplierMax, vein.AmountMultiplierMin, data.RandomMul)}");
        }
    }

    public static void LoadRecipe(Match match, Stream stream)
    {
        BaseRecipe? recipe = JsonSerializer.Deserialize(stream, SourceGenerationContext.Default.BaseRecipe);
        if (recipe is VeinRecipe veinRecipe)
        {
            string id = $"{match.Groups["mod_id"].ValueSpan}:{match.Groups["name"].ValueSpan}";
            veinRecipe.ID = id;
            OreVeinGenerator.Recipes.Add(veinRecipe);
            Console.Error.WriteLine($"Found vein recipe: {id}");
        }
    }

    public static T Read<T>(string prompt, string errorMessage)where T : IParsable<T>
    {
        while (true)
        {
            Console.Error.WriteLine(prompt);
            if (T.TryParse(Console.ReadLine(), null, out T? result))
                return result;
            Console.Error.WriteLine(errorMessage);
        }
    }

    [GeneratedRegex(@"data/(?<mod_id>[^\\]+)/recipes/(?<name>.+)\.json")]
    public static partial Regex VeinRecipePath();
}