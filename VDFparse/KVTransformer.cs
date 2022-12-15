using System.Text.Json;

namespace VDFparse;

public static class KVTransformer
{
    public static void Transform(BinaryReader reader, Utf8JsonWriter writer)
    {
        writer.WriteStartObject("data"u8);
        var startDepth = writer.CurrentDepth;
        while (true)
        {
            var current = (DataType)reader.ReadByte();
            if (current == DataType.END || current == DataType.ENDB)
            {
                writer.WriteEndObject();
                if (writer.CurrentDepth < startDepth)
                    return;
                continue;
            }
            writer.WritePropertyName(ReadString(reader));
            switch (current)
            {
                case DataType.START:
                    writer.WriteStartObject();
                    break;
                case DataType.STRING:
                    writer.WriteStringValue(ReadString(reader));
                    break;
                case DataType.INT:
                    writer.WriteNumberValue(reader.ReadInt32());
                    break;
                case DataType.FLOAT:
                    writer.WriteNumberValue(reader.ReadSingle());
                    break;
                case DataType.PTR:
                    writer.WriteNumberValue(reader.ReadUInt32());
                    break;
                // case DataType.WSTRING:
                //     writer.WriteStringValue(ReadWideString(reader));
                //     break;
                case DataType.COLOR:
                    writer.WriteStringValue($"#{reader.ReadUInt32():X8}");
                    break;
                case DataType.INT64:
                    writer.WriteNumberValue(reader.ReadInt64());
                    break;
                case DataType.UINT64:
                    writer.WriteNumberValue(reader.ReadUInt64());
                    break;
                default:
                    throw new InvalidDataException($"Unexpected type for value ({current})");
            }
        }
    }

    public static void Consume(BinaryReader reader)
    {
        var depth = 0;
        while (true)
        {
            var current = (DataType)reader.ReadByte();
            if (current == DataType.END || current == DataType.ENDB)
            {
                if (depth == 0)
                    return;
                depth--;
                continue;
            }
            while (reader.ReadByte() != 0) { }

            switch (current)
            {
                case DataType.START:
                    depth++;
                    break;
                case DataType.STRING:
                    while (reader.ReadByte() != 0) { }
                    break;
                case DataType.INT:
                    reader.ReadInt32();
                    break;
                case DataType.FLOAT:
                    reader.ReadSingle();
                    break;
                case DataType.PTR:
                    reader.ReadUInt32();
                    break;
                // case DataType.WSTRING:
                //     writer.WriteStringValue(ReadWideString(reader));
                //     break;
                case DataType.COLOR:
                    reader.ReadUInt32();
                    break;
                case DataType.INT64:
                    reader.ReadInt64();
                    break;
                case DataType.UINT64:
                    reader.ReadUInt64();
                    break;
                default:
                    throw new InvalidDataException($"Unexpected type for value ({current})");
            }
        }
    }

    private static byte[] ReadString(BinaryReader reader)
    {
        using var buffer = new MemoryStream();
        byte current;

        while ((current = reader.ReadByte()) != 0)
        {
            buffer.WriteByte(current);
        }

        return buffer.ToArray();
    }
}

internal enum DataType : byte
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
    INT64 = 10,
    ENDB = 11,
}
