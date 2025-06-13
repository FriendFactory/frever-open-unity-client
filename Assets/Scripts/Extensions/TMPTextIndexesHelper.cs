using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Extensions
{
    public static class TMPTextIndexesHelper
    {
        public static List<int> GetExpectedNewLineTMPIndexes(TMP_TextInfo textInfo)
        {
            var lineIndexTMP = new List<int>();
            
            if (textInfo.lineCount <= 1) return lineIndexTMP;
            var lines = textInfo.lineInfo.Take(Mathf.Max(textInfo.lineCount - 1, 0)).ToArray();
            foreach (var line in lines)
            {
                var lastCharacterIndex = line.lastCharacterIndex;
                var lastCharacter = textInfo.characterInfo[lastCharacterIndex];
                if (lastCharacter.character == '\n') continue;
                var expectedNewLinePos =  lastCharacterIndex + 1;
                lineIndexTMP.Add(expectedNewLinePos);
            }

            return lineIndexTMP;
        }
        
        public static List<int> ConvertTMPCharacterInfoToStringIndexes(string text, ICollection<int> tmpIndexes)
        {
            var indexesInString = new List<int>();
            var indexCounter = 0;
            for (var stringIndex = 0; stringIndex < text.Length; stringIndex++)
            {
                if (tmpIndexes.Contains(indexCounter))
                {
                    indexesInString.Add(stringIndex);
                }
                
                indexCounter++;
                
                var currentChar = text[stringIndex];
                var isLastChar = stringIndex == text.Length - 1;
                if (isLastChar) break;
                
                var nextCharIndex = stringIndex + 1;
                var nextChar = text[nextCharIndex];
                if (char.IsSurrogatePair(currentChar, nextChar))
                {
                    stringIndex++; //skip next
                }
            }

            return indexesInString;
        }
    }
}