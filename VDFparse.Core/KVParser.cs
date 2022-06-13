using System.Text;
using System.Drawing;
using System.Text.Json;

namespace VDFparse;

public static class KVParser
{
    public static KVObject Parse(Stream stream)
    {
        using var reader = new BinaryReader(stream);

        return Parse(reader);
    }

    public static KVObject Parse(BinaryReader reader)
    {
        var root = new KVObject();
        var dataStack = new Stack<KVObject>();
        dataStack.Push(root);
        while (true)
        {
            var type = (DataType)reader.ReadByte();
            if (type == DataType.END)
            {
                dataStack.Pop();
                if (dataStack.Count == 0)
                    return root;
                continue;
            }
            var last = dataStack.Peek();
            var name = ReadString(reader);
            if (type == DataType.START)
            {
                var newObj = new KVObject();
                last[name] = newObj;
                dataStack.Push(newObj);
                continue;
            }

            switch (type)
            {
                case DataType.STRING:
                    last[name] = ReadString(reader);
                    break;
                case DataType.INT:
                    last[name] = reader.ReadInt32();
                    break;
                case DataType.FLOAT:
                    last[name] = reader.ReadSingle();
                    break;
                case DataType.PTR:
                    last[name] = reader.ReadUInt32();
                    break;
                case DataType.WSTRING:
                    last[name] = ReadString(reader, wide: true);
                    break;
                case DataType.COLOR:
                    last[name] = Color.FromArgb(reader.ReadInt32());
                    break;
                case DataType.UINT64:
                    last[name] = reader.ReadUInt64();
                    break;
            }
        }
    }

    private static string ReadString(BinaryReader reader, bool wide = false)
    {
        var buffer = new List<byte>();
        byte currentA;
        byte currentB = 0;

        while (true)
        {
            currentA = reader.ReadByte();
            if (wide)
                currentB = reader.ReadByte();
            if (currentA == 0 && currentB == 0)
                break;

            buffer.Add(currentA);
            if (wide)
                buffer.Add(currentB);
        }

        var encoding = wide ? Encoding.Unicode : Encoding.UTF8;
        return encoding.GetString(buffer.ToArray());
    }
}
public class KVObject : Dictionary<string, dynamic>
{
    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}

internal enum DataType
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
}
