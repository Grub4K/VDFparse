using System.Collections.ObjectModel;

namespace VDFparse;
public class AppInfoReader : IVDFFileReader
{
    public uint Magic { get => 0x07_56_44_27; }

    public List<Dataset> Read(BinaryReader reader)
    {
        var Datasets = new List<Dataset>();
        while (true)
        {
            var id = reader.ReadUInt32();
            if (id == 0)
            {
                return Datasets;
            }

            Datasets.Add(new AppDataset(
                ID: id,
                Size: reader.ReadUInt32(),
                InfoState: reader.ReadUInt32(),
                LastUpdated: DateTimeFromUnixTime(reader.ReadUInt32()),
                Token: reader.ReadUInt64(),
                Hash: new ReadOnlyCollection<byte>(reader.ReadBytes(20)),
                ChangeNumber: reader.ReadUInt32(),
                Data: KVParser.Parse(reader)
            ));
        }
    }

    public static DateTime DateTimeFromUnixTime(uint unixTime)
    {
        return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
    }
}

public record AppDataset(uint ID, ReadOnlyCollection<byte> Hash,
    ulong Token, uint ChangeNumber, KVObject Data,
    DateTime LastUpdated, uint InfoState, uint Size)
    : Dataset(ID, Hash, Token, ChangeNumber, Data);
