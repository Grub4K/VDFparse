namespace VDFparse;

public class PackageInfoReader : IVDFFileReader
{
    public uint Magic { get => 0x06_56_55_28; }

    public List<Dataset> Read(BinaryReader reader)
    {
        var datasets = new List<Dataset>();
        while (true)
        {
            var id = reader.ReadUInt32();
            if (id == 0xFFFFFFFF)
                return datasets;

            datasets.Add(new Dataset
            {
                Id = id,
                Hash = reader.ReadBytes(20),
                Token = IsOldVersion ? 0 : reader.ReadUInt64(),
                ChangeNumber = reader.ReadUInt32(),
                Data = KVParser.Parse(reader),
            });
        }
    }

    public static bool IsOldVersion { get => false; }
}

public class PackageInfoReaderOld : PackageInfoReader, IVDFFileReader
{
    public new uint Magic { get => 0x06_56_55_27; }

    public static new bool IsOldVersion { get => true; }
}
