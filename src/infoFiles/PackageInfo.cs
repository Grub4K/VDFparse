using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;



namespace VDFparse
{
    public class Package : Dataset {}

    public class PackageInfoReader : VDFFileReader
    {
        public uint Magic  { get => 0x06_56_55_28; }

        public List<Dataset> Read(BinaryReader reader)
        {
            var Datasets = new List<Dataset>();
            while (true)
            {
                var id = reader.ReadUInt32();
                if (id == 0xFFFFFFFF)
                {
                    return Datasets;
                }

                Datasets.Add(new Package
                {
                    ID = id,
                    Hash = new ReadOnlyCollection<byte>(reader.ReadBytes(20)),
                    Token = IsOldVersion ? 0 : reader.ReadUInt64(),
                    ChangeNumber = reader.ReadUInt32(),
                    Data = KVParser.Parse(reader),
                });
            }
        }

        public bool IsOldVersion { get => false; }
    }

    public class PackageInfoReaderOld : PackageInfoReader
    {
        public new uint Magic  { get => 0x06_56_55_27; }

        public new bool IsOldVersion { get => true; }
    }
}
