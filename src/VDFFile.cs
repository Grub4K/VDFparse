using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;



namespace VDFparse
{
    public enum EUniverse
    {
        Invalid = 0,
        Public = 1,
        Beta = 2,
        Internal = 3,
        Dev = 4,
        Max = 5,
    }

    public interface VDFFileReader
    {
        public uint Magic { get; }
        public List<Dataset> Read(BinaryReader reader);
    }

    public class VDFFile
    {
        private static List<VDFFileReader> Readers = new List<VDFFileReader>(){
            new PackageInfoReaderOld(),
            new PackageInfoReader(),
            new AppInfoReader(),
        };

        public List<Dataset> Datasets { get; private set; } = new List<Dataset>();

        public EUniverse Universe { get; private set; }

        public void Read(string filename)
        {
            using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                Read(fs);
            }
        }

        public void Read(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                var magic = reader.ReadUInt32();
                Universe = (EUniverse)reader.ReadUInt32();
                foreach (var reader_sub in Readers)
                {
                    if (magic == reader_sub.Magic)
                    {
                        Datasets = reader_sub.Read(reader);
                        return;
                    }
                }
                throw new InvalidDataException($"Unknown header: {magic.ToString("X")}");
            }
        }

        public Dataset FindByID(uint id)
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

    public class Dataset
    {
        public uint ID { get; set; }

        public ReadOnlyCollection<byte> Hash { get; set; }

        public ulong Token { get; set; }

        public uint ChangeNumber { get; set; }

        public KVObject Data { get; set; }
    }
}
