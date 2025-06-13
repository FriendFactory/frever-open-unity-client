using System.Text.RegularExpressions;
using AdvancedInputFieldPlugin;
using UnityEngine.Serialization;

namespace UIManaging.Common.InputFields
{
    public class RegexLiveProcessingFilter : CharacterLimitFilter
    {
        public string Regex;
        public override TextEditFrame ProcessTextEditUpdate(TextEditFrame textEditFrame, TextEditFrame lastTextEditFrame)
        {
            var frame = System.Text.RegularExpressions.Regex.IsMatch(textEditFrame.text, Regex) 
                ? textEditFrame 
                : lastTextEditFrame;

            return base.ProcessTextEditUpdate(frame, lastTextEditFrame);
        }
    }
}