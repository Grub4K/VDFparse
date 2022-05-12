using System.Collections.ObjectModel;

namespace VDFparse;

public interface IVDFFileReader
{
    public uint Magic { get; }
    public List<Dataset> Read(BinaryReader reader);
}

public class VDFFile
{
    private static List<IVDFFileReader> VdfFileReaders = new()
    {
        new PackageInfoReaderOld(),
        new PackageInfoReader(),
        new AppInfoReader(),
    };

    public List<Dataset> Datasets { get; private set; } = new();

    public EUniverse Universe { get; private set; }

    public void Read(string filename)
    {
        using var fs = new FileStream(filename, FileMode.Open, FileAccess.Read);

        Read(fs);
    }

    public void Read(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        var magic = reader.ReadUInt32();
        Universe = (EUniverse)reader.ReadUInt32();
        foreach (var vdfFileReader in VdfFileReaders)
        {
            if (magic == vdfFileReader.Magic)
            {
                Datasets = vdfFileReader.Read(reader);
                return;
            }
        }

        throw new InvalidDataException($"Unknown header: {magic:X8}");
    }

    public Dataset? FindByID(uint id)
    {
        foreach (var dataset in Datasets)
        {
            if (dataset.ID == id)
            {
                return dataset;
            }
        }
        return null;
    }
}

public record Dataset(uint ID, ReadOnlyCollection<byte> Hash,
    ulong Token, uint ChangeNumber, KVObject Data);

public enum EUniverse
{
    Invalid = 0,
    Public = 1,
    Beta = 2,
    Internal = 3,
    Dev = 4,
    Max = 5,
}
