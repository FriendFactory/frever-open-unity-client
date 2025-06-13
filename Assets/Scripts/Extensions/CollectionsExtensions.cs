using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extensions
{
    public static class CollectionsExtensions
    {
        public static void ShiftLeft<T>(this T[] array)
        {
            var temp = array[0];
            Array.Copy(array, 1, array, 0, array.Length - 1);
            array[array.Length - 1] = temp;
        }

        public static void ShiftRight<T>(this T[] array)
        {
            var temp = array[array.Length - 1];
            Array.Copy(array, 0, array, 1, array.Length - 1);
            array[0] = temp;
        }
        
        public static string ToStringWithSeparator<T>(this ICollection<T> collection, string separator = ", ")
        {
            var builder = new StringBuilder();

            for (int i = 0; i < collection.Count; i++)
            {
                builder.Append(collection.ElementAt(i));

                if(i != collection.Count - 1)
                {
                    builder.Append(separator);
                }
            }

            return builder.ToString();
        }

        public static T GetRandom<T>(this IEnumerable<T> enumerable)
        {
            var max = enumerable.Count();
            return max == 0
                ? default(T)
                : enumerable.ElementAt(UnityEngine.Random.Range(0, max));
        }
        
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            foreach (var element in source)
            {
                if (seenKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    } 
}