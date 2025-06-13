using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Extensions
{
    public static class StringExtensions
    {
        public static string FirstCharToUpper(this string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));
            return input.First().ToString().ToUpper() + input.Substring(1);
        }

        public static string FirstCharToLower(this string input)
        {
            if (string.IsNullOrEmpty(input))
                throw new ArgumentNullException(nameof(input));
            return input.First().ToString().ToLower() + input.Substring(1);
        }
        
        public static string InsertNewLinesIfNotAdded(this string originalString, ICollection<int> indexes)
        {
            if (!indexes.Any()) return originalString;
            
            var stringBuilder = new StringBuilder(originalString);
            var offset = 0;
            const char newLineChar = '\n';
            foreach (var index in indexes)
            {
                var adjustedIndex = index + offset;
                if (index < originalString.Length && originalString[index] != newLineChar)
                {
                    stringBuilder.Insert(adjustedIndex, newLineChar);
                    offset++;
                }
            }
            return stringBuilder.ToString();
        }
    }
}