using System;
using System.Text;
using System.Linq;
using System.Drawing;
using System.Globalization;
using System.Collections.Generic;
using System.IO;



namespace VDFparse
{
    public enum DataType
    {
        START = 0,
        STRING,
        INT,
        FLOAT,
        PTR,
        WSTRING,
        COLOR,
        UINT64,
        END,
    };

    public static class KVParser
    {
        public static KVObject Parse(Stream stream)
        {
            using (var reader = new BinaryReader(stream))
            {
                return Parse(reader);
            }
        }

        public static KVObject Parse(BinaryReader reader)
        {
            KVObject root = new KVObject();
            var dataStack = new List<KVObject>(){ root };
            while (true)
            {
                var type = reader.ReadByte();
                switch ((DataType)type)
                {
                    case DataType.START:
                        var newObj = new KVObject();
                        dataStack.Last()[ReadString(reader)] = newObj;
                        dataStack.Add(newObj);
                        break;
                    case DataType.END:
                        dataStack.RemoveAt(dataStack.Count - 1);
                        if (dataStack.Count == 0)
                            return root;
                        break;
                    case DataType.STRING:
                        dataStack.Last()[ReadString(reader)] = ReadString(reader);
                        break;
                    case DataType.INT:
                        dataStack.Last()[ReadString(reader)] = reader.ReadInt32();
                        break;
                    case DataType.FLOAT:
                        dataStack.Last()[ReadString(reader)] = reader.ReadSingle();
                        break;
                    case DataType.PTR:
                        dataStack.Last()[ReadString(reader)] = reader.ReadUInt32();
                        break;
                    case DataType.WSTRING:
                        dataStack.Last()[ReadString(reader)] = ReadString(reader, width: 2);
                        break;
                    case DataType.COLOR:
                        dataStack.Last()[ReadString(reader)] = Color.FromArgb(reader.ReadInt32());
                        break;
                    case DataType.UINT64:
                        dataStack.Last()[ReadString(reader)] = reader.ReadUInt64();
                        break;
                }
            }
        }

        // TODO: Correct this to produce correct wide string results
        private static string ReadString(BinaryReader reader, byte width=1)
        {
            StringBuilder builder = new StringBuilder();
            byte[] current = new byte[width];

            while (true)
            {
                for (int i = 0; i < width ; ++i)
                {
                    current[i] = reader.ReadByte();
                }
                if (current.All(val => val == 0))
                {
                    break;
                }
                foreach (var c in current)
                {
                    builder.Append((char)c);
                }
            }
            return builder.ToString();
        }
    }
}
