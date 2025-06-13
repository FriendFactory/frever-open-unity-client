using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace UMA.CharacterSystem
{
    public static class UMAExtensions
    {
        public static int WordCount(this string str)
        {
            return str.Split(new char[] { ' ', '.', '?' },
                             StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public static string[] SplitCamelCase(this string str)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (i > 0 && char.IsUpper(c)) sb.Append('|');
                if (i == 0)
                    c = char.ToUpper(c);
                sb.Append(c);
            }

            return sb.ToString().Split('|');
        }

        public static string BreakupCamelCase(this string str)
        {
            var sb = new StringBuilder();
            for (var i = 0; i < str.Length; i++)
            {
                var c = str[i];
                if (i > 0 && char.IsUpper(c)) sb.Append(' ');
                if (i == 0)
                    c = char.ToUpper(c);
                sb.Append(c);
            }

            return sb.ToString();
        }
        
        public static Dictionary<string, Color32> GetColors(this DynamicCharacterAvatar avatar)
        {
            var avatarColor = new Dictionary<string, Color32>();
            if (avatar == null || avatar.characterColors == null) return avatarColor;

            foreach (var color in avatar.characterColors.Colors)
            {
                avatarColor.Add(color.Name, color.Color);
            }
            return avatarColor;
        }
    }
}