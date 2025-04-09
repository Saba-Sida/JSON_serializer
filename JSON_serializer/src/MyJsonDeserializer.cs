using System.Collections;
using System.Globalization;
using System.Reflection;

namespace JSON_serializer.src
{
    public static partial class MyJsonSerializer
    {
        public static T Deserialize<T>(string jsonData)
        {
            int index = 0;
            jsonData = GetBetterString(jsonData);
            object objectStructure = ParseValue(jsonData, ref index);
            return GetMappedObject<T>(objectStructure);
        }

        #region Assist Private Methods For Deserialization
        public static string GetBetterString(string data)
        {
            return data.Replace("\n", "")
                .Replace("\r", "");

        }
        private static object ParseValue(string jsonData, ref int index)
        {
            SkipWhiteSpaces(jsonData, ref index);

            if (jsonData[index] == '{') return ParseObject(jsonData, ref index);
            if (jsonData[index] == '[') return ParseArray(jsonData, ref index);
            if (jsonData[index] == '"') return ParseString(jsonData, ref index);
            if (jsonData.Substring(index).StartsWith("true")) return ParseLiteral(jsonData, ref index, "true", true);
            if (jsonData.Substring(index).StartsWith("false")) return ParseLiteral(jsonData, ref index, "false", false);
            if (jsonData.Substring(index).StartsWith("null")) return ParseLiteral(jsonData, ref index, "null", null);
            if (jsonData[index] == '-' || (jsonData[index] >= '0' && jsonData[index] <= '9')) return ParseNumber(jsonData, ref index);


            return null!;
        }
        private static void SkipWhiteSpaces(string jsonData, ref int index)
        {
            while (jsonData[index] == ' ') index++;
        }
        private static string ParseString(string jsonData, ref int index)
        {
            index++;
            int startIndex = index;
            while (jsonData[index] != '"') index++;

            string stringValue = jsonData.Substring(startIndex, index - startIndex);
            index++;

            return stringValue;
        }
        private static object ParseLiteral(string jsonData, ref int index, string literal, object literalParse)
        {
            index += literal.Length;
            return literalParse;
        }
        private static Dictionary<string, object> ParseObject(string jsonData, ref int index)
        {
            index++;
            Dictionary<string, object> dictionary = new();

            while (jsonData[index] != '}')
            {
                SkipWhiteSpaces(jsonData, ref index);
                string key = ParseString(jsonData, ref index);
                SkipWhiteSpaces(jsonData, ref index);

                if (jsonData[index] != ':') throw new Exception("Object key requires following ':' symbol before the value!!!");
                index++;
                SkipWhiteSpaces(jsonData, ref index);

                object value = ParseValue(jsonData, ref index);
                SkipWhiteSpaces(jsonData, ref index);

                if (jsonData[index] == ',') index++;

                dictionary[key] = value;
            }
            index++;
            return dictionary;
        }
        private static List<object> ParseArray(string jsonData, ref int index)
        {
            index++;
            List<object> list = new();
            while (jsonData[index] != ']')
            {
                SkipWhiteSpaces(jsonData, ref index);
                object item = ParseValue(jsonData, ref index);
                list.Add(item);
                SkipWhiteSpaces(jsonData, ref index);
                if (jsonData[index] == ',') index++;
            }
            index++;
            return list;
        }
        private static object ParseNumber(string jsonData, ref int index)
        {
            int startIndex = index;
            while (index < jsonData.Length && (jsonData[index] == '-' || jsonData[index] == '.' || (jsonData[index] >= '0' && jsonData[index] <= '9'))) index++;
            string parsedString = jsonData.Substring(startIndex, index - startIndex);

            if (parsedString.Contains('.')) return double.Parse(parsedString, CultureInfo.InvariantCulture);
            else return long.Parse(parsedString);
        }

        private static T GetMappedObject<T>(object objectStructure)
        {
            return (T)ConvertToType(objectStructure, typeof(T));
        }

        private static object ConvertToType(object someObject, Type targetType)
        {

            if (someObject == null) return null!;

            // Handle primitives
            if (targetType == typeof(string)) return someObject.ToString()!;
            if (targetType.IsPrimitive || targetType == typeof(decimal) || targetType == typeof(double) || targetType == typeof(float) || targetType == typeof(long) || targetType == typeof(int))
                return Convert.ChangeType(someObject, targetType, CultureInfo.InvariantCulture);

            // Handle Nullable<T>
            if (Nullable.GetUnderlyingType(targetType) != null)
            {
                Type underlying = Nullable.GetUnderlyingType(targetType)!;
                return ConvertToType(someObject, underlying);
            }

            // Handle List<T>
            if (targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type itemType = targetType.GetGenericArguments()[0];
                List<object> inputList = (List<object>)someObject;
                IList resultList = (IList)Activator.CreateInstance(targetType)!;
                foreach (var item in inputList)
                {
                    resultList.Add(ConvertToType(item, itemType));
                }
                return resultList;
            }

            // Handle Dictionary<string, object> to class mapping
            if (someObject is Dictionary<string, object> dict)
            {
                object instance = Activator.CreateInstance(targetType)!;

                foreach (PropertyInfo prop in targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (!prop.CanWrite) continue;
                    if (!dict.TryGetValue(prop.Name, out var rawVal)) continue;
                    if (rawVal == null) continue;

                    object converted = ConvertToType(rawVal, prop.PropertyType);
                    prop.SetValue(instance, converted);
                }

                return instance;
            }

            // Fallback
            return someObject;
        }
        #endregion
    }
}
