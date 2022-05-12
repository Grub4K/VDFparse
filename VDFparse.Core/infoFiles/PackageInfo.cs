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

            datasets.Add(new Dataset(
                ID: id,
                Hash: Array.AsReadOnly(reader.ReadBytes(20)),
                Token: IsOldVersion ? 0 : reader.ReadUInt64(),
                ChangeNumber: reader.ReadUInt32(),
                Data: KVParser.Parse(reader)
            ));
        }
    }

    public bool IsOldVersion { get => false; }
}

public class PackageInfoReaderOld : PackageInfoReader
{
    public new uint Magic { get => 0x06_56_55_27; }

    public new bool IsOldVersion { get => true; }
}
