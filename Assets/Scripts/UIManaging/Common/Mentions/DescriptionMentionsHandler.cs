using System;
using System.Text.RegularExpressions;
using UIManaging.Common.InputFields;
using UnityEngine;
using static UIManaging.Common.Ui.MentionsPanel;

namespace UIManaging.Common.Ui
{
    public sealed class DescriptionMentionsHandler
    {
        private readonly IInputFieldAdapter _inputFieldAdapter;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action MentionRequested;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public DescriptionMentionsHandler(IInputFieldAdapter inputFieldAdapter)
        {
            _inputFieldAdapter = inputFieldAdapter;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void HandleMentions(string oldText)
        {
            var newText = _inputFieldAdapter.Text;

            var stringPosition = _inputFieldAdapter.StringPosition;
            if (stringPosition > 0 && newText[stringPosition - 1].Equals('@'))
            {
                MentionRequested?.Invoke();
            }

            var matches = Regex.Matches(oldText, MENTION_REGEX);
            if (matches.Count > 0)
            {
                var diffLenght = newText.Length - oldText.Length;
                var diffStartIndex = DiffersAtIndex(oldText, newText);
                var diffEndIndex = diffStartIndex + Mathf.Abs(diffLenght);
            
                if (diffLenght < 0)
                {
                    OnCharactersDeleted(oldText, matches, diffStartIndex, diffEndIndex);
                }
                else
                {
                    OnCharactersAdded(oldText, matches, diffStartIndex, diffLenght);
                }
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnCharactersDeleted(string oldText, MatchCollection matches, int diffStartIndex, int diffEndIndex)
        {
            var adjustDiff = false;

            foreach (Match match in matches)
            {
                var matchStart = match.Index;
                var matchEnd = match.Index + match.Length;
                    
                if (matchStart < diffStartIndex && diffStartIndex < matchEnd)
                {
                    diffStartIndex = matchStart;
                    adjustDiff = true;
                }
                
                if (matchStart < diffEndIndex && diffEndIndex < matchEnd)
                {
                    diffEndIndex = matchEnd;
                    adjustDiff = true;
                }
            }

            if (adjustDiff)
            {
                var newText = oldText.Remove(diffStartIndex, diffEndIndex - diffStartIndex);
                _inputFieldAdapter.SetTextWithoutNotify(newText);
                _inputFieldAdapter.StringPosition = diffStartIndex;
                _inputFieldAdapter.CaretPosition =
                    _inputFieldAdapter.GetCaretPositionFromStringIndex(_inputFieldAdapter.StringPosition);
            }
        }

        private void OnCharactersAdded(string oldText, MatchCollection matches, int diffStartIndex, int diffLenght)
        {
            foreach (Match match in matches)
            {
                var matchStart = match.Index;
                var matchEnd = match.Index + match.Length;
                    
                if (matchStart < diffStartIndex && diffStartIndex < matchEnd)
                {
                    var diffString = _inputFieldAdapter.Text.Substring(diffStartIndex, diffLenght);
                    var newText = oldText.Insert(matchEnd, diffString);
                    _inputFieldAdapter.SetTextWithoutNotify(newText);
                    _inputFieldAdapter.StringPosition = matchEnd + diffLenght;
                    _inputFieldAdapter.CaretPosition =
                        _inputFieldAdapter.GetCaretPositionFromStringIndex(_inputFieldAdapter.StringPosition);
                    break;
                }
            }
        }

        /// <summary>
        /// Compare two strings and return the index of the first difference.  Return -1 if the strings are equal.
        /// </summary>
        private static int DiffersAtIndex(string s1, string s2)
        {
            var index = 0;
            var min = Mathf.Min(s1.Length, s2.Length);
            
            while (index < min && s1[index] == s2[index])
            {
                index++;
            }

            return (index == min && s1.Length == s2.Length) ? -1 : index;
        }
    }
}