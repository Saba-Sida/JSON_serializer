using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace JSON_serializer.src
{
    public static class AdditiveFunctions
    {

        public static void Print(this object someObject, int depth = 0)
        {
            string GetDepthSpace()
            {
                return new string('\t', depth);
            }

            if (someObject == null)
            {
                Console.WriteLine($"{GetDepthSpace()}null");
                return;
            }

            Type objectType = someObject.GetType();

            if (objectType.IsPrimitive || someObject is string || someObject is decimal)
            {
                Console.WriteLine($"{GetDepthSpace()}{someObject}");
                return;
            }

            if (someObject is IEnumerable enumerable)
            {
                Console.WriteLine($"{GetDepthSpace()}Collection of {objectType}:");

                foreach (var item in enumerable)
                {
                    item.Print(depth + 1);
                }
                return;
            }

            if (someObject is IDictionary dictionary)
            {
                Console.WriteLine($"{GetDepthSpace()}Dictionary of {objectType}:");

                foreach (DictionaryEntry entry in dictionary)
                {
                    Console.WriteLine($"{GetDepthSpace()}\tKey:");
                    entry.Key.Print(depth + 2);
                    Console.WriteLine($"{GetDepthSpace()}\tValue:");
                    entry.Value.Print(depth + 2);
                }
                return;
            }

            Console.WriteLine($"{GetDepthSpace()}Object of {objectType}:");

            foreach (var property in objectType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                try
                {
                    var propertyValue = property.GetValue(someObject, null);
                    Console.WriteLine($"{GetDepthSpace()}\t{property.Name}:");
                    propertyValue.Print(depth + 1);
                }
                catch
                {
                    Console.WriteLine($"{GetDepthSpace()}\t{property.Name}: [Error getting value]");
                }
            }
        }

    }
}
