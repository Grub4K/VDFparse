using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;



namespace VDFparse
{
    public class AppInfoReader : VDFFileReader
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

                Datasets.Add(new App
                {
                    ID = id,
                    Size = reader.ReadUInt32(),
                    InfoState = reader.ReadUInt32(),
                    LastUpdated = DateTimeFromUnixTime(reader.ReadUInt32()),
                    Token = reader.ReadUInt64(),
                    Hash = new ReadOnlyCollection<byte>(reader.ReadBytes(20)),
                    ChangeNumber = reader.ReadUInt32(),
                    Data = KVParser.Parse(reader),
                });
            }
        }

        public static DateTime DateTimeFromUnixTime(uint unixTime)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixTime);
        }
    }

    public class App : Dataset
    {
        public uint Size { get; set; }

        public uint InfoState { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
