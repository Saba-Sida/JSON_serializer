using System.Collections;
using System.Text;

namespace JSON_serializer.src
{
    public static partial class MyJsonSerializer
    {
        public static string Serialize(Object? o)
        {
            StringBuilder jsonResultBuilder = new StringBuilder();
            if (IsPrimitiveOrString(o)) return SerializePrimitive(o);
            else if (o is null) return "null";
            else
            {
                if (o is not IEnumerable)
                {
                    // just object

                    jsonResultBuilder.Append("{");
                    var properties = o.GetType().GetProperties();
                    foreach (var property in properties)
                    {
                        jsonResultBuilder.Append($"\"{property.Name}\":{Serialize(property.GetValue(o))},");
                    }
                    if (jsonResultBuilder[^1] == ',') jsonResultBuilder.Length--;
                    jsonResultBuilder.Append("}");
                    return jsonResultBuilder.ToString();
                }
                else
                {
                    // case of IEnumerable

                    if (o is IDictionary dictionary)
                    {
                        jsonResultBuilder.Append("{");
                        var properties = o.GetType().GetProperties();
                        foreach (DictionaryEntry entry in dictionary)
                        {
                            jsonResultBuilder.Append($"\"{entry.Key}\":{Serialize(entry.Value)},");
                        }
                        if (jsonResultBuilder[^1] == ',') jsonResultBuilder.Length--;
                        jsonResultBuilder.Append("}");
                        return jsonResultBuilder.ToString();
                    }
                    jsonResultBuilder.Append("[");
                    var objects = (IEnumerable)o;
                    foreach (var obj in objects)
                    {
                        jsonResultBuilder.Append(Serialize(obj));
                        jsonResultBuilder.Append(',');
                    }
                    if (jsonResultBuilder[^1] == ',') jsonResultBuilder.Length--;
                    jsonResultBuilder.Append("]");
                }
                return jsonResultBuilder.ToString();
            }

        }

        #region Assist Private Methods For Serialization

        private static bool IsNumber(object item)
        {
            return item is byte or sbyte
                or short or ushort
                or int or uint
                or long or ulong
                or float or double
                or decimal;
        }
        private static bool IsPrimitiveOrString(object item)
        {
            return IsNumber(item) || (item is char or string or bool);
        }
        private static string SerializePrimitive(Object o)
        {
            StringBuilder jsonResultBuilder = new StringBuilder();
            if (o is string or char)
            {
                jsonResultBuilder.Append($"\"{ConvertToStringWithoutSpecChars(o.ToString()!)}\"");
            }
            else if (IsNumber(o))
            {
                jsonResultBuilder.Append(o);
            }
            else if (o is bool)
            {
                jsonResultBuilder.Append(((bool)o ? "true" : "false"));
            }
            return jsonResultBuilder.ToString();
        }
        private static string ConvertToStringWithoutSpecChars(string stringWithSpecChars)
        {
            return stringWithSpecChars.Replace("\\", "\\\\")
                                      .Replace("\"", "\\\"")
                                      .Replace("\n", "\\n")
                                      .Replace("\r", "\\r")
                                      .Replace("\b", "\\b")
                                      .Replace("\f", "\\f")
                                      .Replace("\a", "\\a");
        }
        #endregion
    }
}
