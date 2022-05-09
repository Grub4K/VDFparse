using System.Text;
using System.Drawing;

namespace VDFparse;

public class KVObject : Dictionary<string, dynamic>, IFormattable
{
    public override string ToString()
    {
        return this.ToString("0");
    }

    public string ToString(string? format, IFormatProvider? formatProvider = null)
    {
        uint indent = 0;
        if (String.IsNullOrEmpty(format))
        {
            indent = 0;
        }
        else
        {
            bool success = UInt32.TryParse(format, out indent);
            if (!success)
            {
                throw new FormatException($"The '{format}' format string is not supported.");
            }
        }

        return ToJSON((int)indent);
    }

    public string ToJSON(int indent)
    {
        var builder = new StringBuilder();
        ToJSON(builder, indent);
        return builder.ToString();
    }

    private void ToJSON(StringBuilder builder, int indent, int depth = 0)
    {
        bool indented = indent != 0;
        if (Count == 0)
        {
            builder.Append("{}");
            return;
        }
        builder.Append('{');
        if (indented)
            builder.Append('\n');
        using var enumerator = this.GetEnumerator();
        var notLast = enumerator.MoveNext();
        KeyValuePair<string, dynamic> keyValue;
        while (notLast)
        {
            keyValue = enumerator.Current;
            if (indented)
                builder.Append(new String(' ', indent * (depth + 1)));
            builder.Append('"').Append(keyValue.Key).Append("\":");
            if (indented)
                builder.Append(' ');
            Type type = keyValue.Value.GetType();
            if (type == typeof(String))
                builder.Append('"').Append(keyValue.Value).Append('"');
            else if (type == typeof(KVObject))
                keyValue.Value.ToJSON(builder, indent, depth + 1);
            else if (type == typeof(Color))
                builder.Append(keyValue.Value.ToArgb());
            else
                builder.Append(keyValue.Value);
            notLast = enumerator.MoveNext();
            if (notLast)
            {
                builder.Append(',');
                if (indent != 0)
                    builder.Append('\n');
            }
        }
        if (indented)
            builder
                .Append('\n')
                .Append(new String(' ', indent * depth));
        builder.Append('}');
    }

    public IEnumerable<dynamic> Search(string fullquery, bool sub = false)
    {
        var query = fullquery.Split(new[] { '.' }, 2);
        foreach (var keyValue in this)
        {
            var isKVObject = keyValue.Value.GetType() == typeof(KVObject);
            if (query[0] == "*" || keyValue.Key == query[0])
            {
                if (query.Length == 1)
                {
                    yield return keyValue.Value;
                }
                else
                {
                    if (isKVObject)
                    {
                        foreach (var subobj in keyValue.Value.Search(query[1], sub: true))
                        {
                            yield return subobj;
                        }
                    }
                }
            }
            if (!sub && isKVObject)
            {
                foreach (var subobj in keyValue.Value.Search(fullquery))
                {
                    yield return subobj;
                }
            }
        }
    }
}
