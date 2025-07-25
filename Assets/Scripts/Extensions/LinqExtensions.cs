using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
    /// <summary>Various LinQ extensions.</summary>
    /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions">`LinqExtensions` on google.com</a></footer>
    public static class LinqExtensions
    {
        /// <summary>Calls an action on each item before yielding them.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="action">The action to call for each item.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.Examine">`LinqExtensions.Examine` on google.com</a></footer>
        public static IEnumerable<T> Examine<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T obj in source)
            {
                action(obj);
                yield return obj;
            }
        }

        /// <summary>Perform an action on each item.</summary>
        /// <param name="source">The source.</param>
        /// <param name="action">The action to perform.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.ForEach">`LinqExtensions.ForEach` on google.com</a></footer>
        public static IEnumerable<T> ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T obj in source)
                action(obj);
            return source;
        }

        /// <summary>Perform an action on each item.</summary>
        /// <param name="source">The source.</param>
        /// <param name="action">The action to perform.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.ForEach">`LinqExtensions.ForEach` on google.com</a></footer>
        public static IEnumerable<T> ForEach<T>(
            this IEnumerable<T> source,
            Action<T, int> action)
        {
            int num = 0;
            foreach (T obj in source)
                action(obj, num++);
            return source;
        }

        /// <summary>Convert each item in the collection.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="converter">Func to convert the items.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.Convert">`LinqExtensions.Convert` on google.com</a></footer>
        public static IEnumerable<T> Convert<T>(
            this IEnumerable source,
            Func<object, T> converter)
        {
            foreach (object obj in source)
                yield return converter(obj);
        }

        /// <summary>Convert a colletion to a HashSet.</summary>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.ToHashSet">`LinqExtensions.ToHashSet` on google.com</a></footer>
        public static HashSet<T> ToHashSet<T>(
            this IEnumerable<T> source,
            IEqualityComparer<T> comparer)
        {
            return new HashSet<T>(source, comparer);
        }

        /// <summary>Add an item to the beginning of a collection.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="prepend">Func to create the item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependWith">`LinqExtensions.PrependWith` on google.com</a></footer>
        public static IEnumerable<T> PrependWith<T>(
            this IEnumerable<T> source,
            Func<T> prepend)
        {
            yield return prepend();
            foreach (T obj in source)
                yield return obj;
        }

        /// <summary>Add an item to the beginning of a collection.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="prepend">The item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependWith">`LinqExtensions.PrependWith` on google.com</a></footer>
        public static IEnumerable<T> PrependWith<T>(this IEnumerable<T> source, T prepend)
        {
            yield return prepend;
            foreach (T obj in source)
                yield return obj;
        }

        /// <summary>
        /// Add a collection to the beginning of another collection.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="prepend">The collection to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependWith">`LinqExtensions.PrependWith` on google.com</a></footer>
        public static IEnumerable<T> PrependWith<T>(
            this IEnumerable<T> source,
            IEnumerable<T> prepend)
        {
            foreach (T obj in prepend)
                yield return obj;
            foreach (T obj in source)
                yield return obj;
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">Func to create the item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            bool condition,
            Func<T> prepend)
        {
            if (condition)
                yield return prepend();
            foreach (T obj in source)
                yield return obj;
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            bool condition,
            T prepend)
        {
            if (condition)
                yield return prepend;
            foreach (T obj in source)
                yield return obj;
        }

        /// <summary>
        /// Add a collection to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The collection to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            bool condition,
            IEnumerable<T> prepend)
        {
            if (condition)
            {
                foreach (T obj in prepend)
                    yield return obj;
            }
            foreach (T obj in source)
                yield return obj;
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">Func to create the item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            Func<T> prepend)
        {
            if (condition())
                yield return prepend();
            foreach (T obj in source)
                yield return obj;
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            T prepend)
        {
            if (condition())
                yield return prepend;
            foreach (T obj in source)
                yield return obj;
        }

        /// <summary>
        /// Add a collection to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The collection to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            IEnumerable<T> prepend)
        {
            if (condition())
            {
                foreach (T obj in prepend)
                    yield return obj;
            }
            foreach (T obj in source)
                yield return obj;
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">Func to create the item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<IEnumerable<T>, bool> condition,
            Func<T> prepend)
        {
            if (condition(source))
                yield return prepend();
            foreach (T obj in source)
                yield return obj;
        }

        /// <summary>
        /// Add an item to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The item to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<IEnumerable<T>, bool> condition,
            T prepend)
        {
            if (condition(source))
                yield return prepend;
            foreach (T obj in source)
                yield return obj;
        }

        /// <summary>
        /// Add a collection to the beginning of another collection, if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="prepend">The collection to prepend.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.PrependIf">`LinqExtensions.PrependIf` on google.com</a></footer>
        public static IEnumerable<T> PrependIf<T>(
            this IEnumerable<T> source,
            Func<IEnumerable<T>, bool> condition,
            IEnumerable<T> prepend)
        {
            if (condition(source))
            {
                foreach (T obj in prepend)
                    yield return obj;
            }
            foreach (T obj in source)
                yield return obj;
        }

        /// <summary>Add an item to the end of a collection.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="append">Func to create the item to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendWith">`LinqExtensions.AppendWith` on google.com</a></footer>
        public static IEnumerable<T> AppendWith<T>(
            this IEnumerable<T> source,
            Func<T> append)
        {
            foreach (T obj in source)
                yield return obj;
            yield return append();
        }

        /// <summary>Add an item to the end of a collection.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="append">The item to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendWith">`LinqExtensions.AppendWith` on google.com</a></footer>
        public static IEnumerable<T> AppendWith<T>(this IEnumerable<T> source, T append)
        {
            foreach (T obj in source)
                yield return obj;
            yield return append;
        }

        /// <summary>Add a collection to the end of another collection.</summary>
        /// <param name="source">The collection.</param>
        /// <param name="append">The collection to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendWith">`LinqExtensions.AppendWith` on google.com</a></footer>
        public static IEnumerable<T> AppendWith<T>(
            this IEnumerable<T> source,
            IEnumerable<T> append)
        {
            foreach (T obj in source)
                yield return obj;
            foreach (T obj in append)
                yield return obj;
        }

        /// <summary>
        /// Add an item to the end of a collection if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="append">Func to create the item to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendIf">`LinqExtensions.AppendIf` on google.com</a></footer>
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            bool condition,
            Func<T> append)
        {
            foreach (T obj in source)
                yield return obj;
            if (condition)
                yield return append();
        }

        /// <summary>
        /// Add an item to the end of a collection if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="append">The item to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendIf">`LinqExtensions.AppendIf` on google.com</a></footer>
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            bool condition,
            T append)
        {
            foreach (T obj in source)
                yield return obj;
            if (condition)
                yield return append;
        }

        /// <summary>
        /// Add a collection to the end of another collection if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="append">The collection to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendIf">`LinqExtensions.AppendIf` on google.com</a></footer>
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            bool condition,
            IEnumerable<T> append)
        {
            foreach (T obj in source)
                yield return obj;
            if (condition)
            {
                foreach (T obj in append)
                    yield return obj;
            }
        }

        /// <summary>
        /// Add an item to the end of a collection if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="append">Func to create the item to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendIf">`LinqExtensions.AppendIf` on google.com</a></footer>
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            Func<T> append)
        {
            foreach (T obj in source)
                yield return obj;
            if (condition())
                yield return append();
        }

        /// <summary>
        /// Add an item to the end of a collection if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="append">The item to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendIf">`LinqExtensions.AppendIf` on google.com</a></footer>
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            T append)
        {
            foreach (T obj in source)
                yield return obj;
            if (condition())
                yield return append;
        }

        /// <summary>
        /// Add a collection to the end of another collection if a condition is met.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <param name="condition">The condition.</param>
        /// <param name="append">The collection to append.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AppendIf">`LinqExtensions.AppendIf` on google.com</a></footer>
        public static IEnumerable<T> AppendIf<T>(
            this IEnumerable<T> source,
            Func<bool> condition,
            IEnumerable<T> append)
        {
            foreach (T obj in source)
                yield return obj;
            if (condition())
            {
                foreach (T obj in append)
                    yield return obj;
            }
        }

        /// <summary>
        /// Returns and casts only the items of type <typeparamref name="T" />.
        /// </summary>
        /// <param name="source">The collection.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.FilterCast">`LinqExtensions.FilterCast` on google.com</a></footer>
        public static IEnumerable<T> FilterCast<T>(this IEnumerable source)
        {
            foreach (object obj1 in source)
            {
                if (obj1 is T obj)
                    yield return obj;
            }
        }

        /// <summary>Adds a collection to a hashset.</summary>
        /// <param name="hashSet">The hashset.</param>
        /// <param name="range">The collection.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AddRange">`LinqExtensions.AddRange` on google.com</a></footer>
        public static void AddRange<T>(this HashSet<T> hashSet, IEnumerable<T> range)
        {
            foreach (T obj in range)
                hashSet.Add(obj);
        }

        /// <summary>
        /// Returns <c>true</c> if the collection is either null or empty. Otherwise <c>false</c>.
        /// </summary>
        /// <param name="collection">The list.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.IsNullOrEmpty">`LinqExtensions.IsNullOrEmpty` on google.com</a></footer>
        public static bool IsNullOrEmpty<T>(this ICollection<T> collection) => collection == null || collection.Count == 0;

        /// <summary>Sets all items in the list to the given value.</summary>
        /// <param name="list">The list.</param>
        /// <param name="item">The value.</param>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.Populate">`LinqExtensions.Populate` on google.com</a></footer>
        public static void Populate<T>(this IList<T> list, T item)
        {
            int count = list.Count;
            for (int index = 0; index < count; ++index)
                list[index] = item;
        }

        /// <summary>
        /// Adds the elements of the specified collection to the end of the IList&lt;T&gt;.
        /// </summary>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.AddRange">`LinqExtensions.AddRange` on google.com</a></footer>
        public static void AddRange<T>(this IList<T> list, IEnumerable<T> collection)
        {
            if (list is List<T>)
            {
                ((List<T>) list).AddRange(collection);
            }
            else
            {
                foreach (T obj in collection)
                    list.Add(obj);
            }
        }

        /// <summary>Sorts an IList</summary>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.Sort">`LinqExtensions.Sort` on google.com</a></footer>
        public static void Sort<T>(this IList<T> list, Comparison<T> comparison)
        {
            if (list is List<T>)
            {
                ((List<T>) list).Sort(comparison);
            }
            else
            {
                List<T> objList = new List<T>((IEnumerable<T>) list);
                objList.Sort(comparison);
                for (int index = 0; index < list.Count; ++index)
                    list[index] = objList[index];
            }
        }

        /// <summary>Sorts an IList</summary>
        /// <footer><a href="https://www.google.com/search?q=Sirenix.Utilities.LinqExtensions.Sort">`LinqExtensions.Sort` on google.com</a></footer>
        public static void Sort<T>(this IList<T> list)
        {
            if (list is List<T>)
            {
                ((List<T>) list).Sort();
            }
            else
            {
                List<T> objList = new List<T>((IEnumerable<T>) list);
                objList.Sort();
                for (int index = 0; index < list.Count; ++index)
                    list[index] = objList[index];
            }
        }

        public static IEnumerable<T> TakeLast<T>(this IEnumerable<T> source, int count)
        {
            var enumerable = source as T[] ?? source.ToArray();
            return enumerable.Skip(Math.Max(0, enumerable.Count() - count));
        }

        public static T[] RemoveNulls<T>(this T[] array) where T: class
        {
            if (array == null) return null;
            return array.All(x => x != null) ? array : array.Where(x => x != null).ToArray();
        }
    }
}