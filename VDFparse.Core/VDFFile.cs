namespace VDFparse;

public interface IVDFFileReader
{
    public uint Magic { get; }
    public List<Dataset> Read(BinaryReader reader);
}

public class VDFFile
{
    VDFFile() { }

    public static List<IVDFFileReader> Readers { get; } = new()
    {
        new AppInfoReader(),
        new PackageInfoReader(),
        new PackageInfoReaderOld(),
    };

    public List<Dataset> Datasets { get; private set; } = new();

    public EUniverse EUniverse { get; private set; }

    public static VDFFile Read(string filename)
    {
        using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

        return Read(fs);
    }

    public static VDFFile Read(Stream stream)
    {
        var vdfFile = new VDFFile();

        using var reader = new BinaryReader(stream);

        var magic = reader.ReadUInt32();
        vdfFile.EUniverse = (EUniverse)reader.ReadUInt32();
        foreach (var vdfFileReader in Readers)
        {
            if (magic == vdfFileReader.Magic)
            {
                vdfFile.Datasets = vdfFileReader.Read(reader);
                return vdfFile;
            }
        }

        throw new InvalidDataException($"Unknown header: {magic:X8}");
    }
}

public class Dataset
{
    public uint Id { get; init; }
    public byte[] Hash { get; init; } = null!;
    public ulong Token { get; init; }
    public uint ChangeNumber { get; init; }
    public KVObject Data { get; init; } = null!;
}

public enum EUniverse
{
    Invalid = 0,
    Public = 1,
    Beta = 2,
    Internal = 3,
    Dev = 4,
    Max = 5,
}
