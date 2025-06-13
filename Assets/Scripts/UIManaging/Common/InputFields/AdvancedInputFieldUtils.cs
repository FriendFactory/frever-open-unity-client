using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AdvancedInputFieldPlugin;
using Bridge.Models.VideoServer;
using Bridge.VideoServer;
using UIManaging.Common.Hashtags;
using UIManaging.Common.Ui;


namespace UIManaging.Common.InputFields
{
    public static class AdvancedInputFieldUtils
    {
        // should be aligned with UserNameValidator regex
        private const string MENTION_PATTERN = "<link=\"mention:([0-9]+)\"><style=\"Mention\">(@[\\w-._@+]+)</style></link>";
        private const string HASHTAG_PATTERN = "<link=\"hashtag:([0-9]+)\"><b>(#\\w{1,25})</b></link>";
        private static readonly Regex _stripWhitespace = new Regex(@"\s+");

        public static RichTextBindingData GetHashtagBindingData(HashtagInfo hashtagInfo)
        {
            // TODO: create single point to generate RT binding data FREV-12252
            var tagName = $"#{hashtagInfo.Name}";
            var richText = string.Format(HashtagsPanel.HASHTAG_TAG, tagName, hashtagInfo.Id);
            var tagData = new RichTextBindingData(tagName, richText);
            var hasBinding = NativeKeyboardManager.RichTextBindingEngine.TryGetBindingFromName(tagData.name, out var _);
            if (!hasBinding)
            {
                NativeKeyboardManager.RichTextBindingEngine.AddBinding(tagData);
            }

            NativeKeyboardManager.RichTextBindingEngine.TryGetBindingFromName(tagData.name, out tagData);

            return tagData;
        }

        public static RichTextBindingData GetMentionBindingData(Mention mention)
        {
            var mentionName = $"@{NormalizeUserName(mention.Name)}";
            
            var richText = string.Format(MentionsPanel.MENTION_TAG, mention.GroupId, mentionName);
            var tagData = new RichTextBindingData(mentionName, richText);
            var hasBinding = NativeKeyboardManager.RichTextBindingEngine.TryGetBindingFromName(tagData.name, out var _);
            if (!hasBinding)
            {
                NativeKeyboardManager.RichTextBindingEngine.AddBinding(tagData);
            }

            NativeKeyboardManager.RichTextBindingEngine.TryGetBindingFromName(tagData.name, out tagData);
            
            return tagData;
        }

        // DWC-2021.3.18 upgrade: moved from old custom version of TMP to here to replace TMP_TextUtilities.GetParsedText
        private const string TAGS_REGEX =
            @"<\/?align.*?>|" +
            @"<\/?allcaps>|" +
            @"<\/?alpha.*?>|" +
            @"<\/?b>|" +
            @"<\/?b=.*?>|" +
            @"<\/?color.*?>|" +
            @"<\/?cspace.*?>|" +
            @"<\/?font.*?>|" +
            @"<\/?font-weight.*?>|" +
            @"<\/?gradient.*?>|" +
            @"<\/?i>|" +
            @"<\/?indent.*?>|" +
            @"<\/?line-height.*?>|" +
            @"<\/?line-indent.*?>|" +
            @"<\/?line-indent.*?>|" +
            @"<\/?link.*?>|" +
            @"<\/?lowercase>|" +
            @"<\/?margin.*?>|" +
            @"<\/?mark.*?>|" +
            @"<\/?mspace.*?>|" +
            @"<\/?nobr>|" +
            @"<\/?noparse>|" +
            @"<\/?page>|" +
            @"<\/?pos.*?>|" +
            @"<\/?rotate.*?>|" +
            @"<\/?s>|" +
            @"<\/?size.*?>|" +
            @"<\/?smallcaps>|" +
            @"<\/?space.*?>|" +
            @"<\/?sprite.*?>|" +
            @"<\/?style.*?>|" +
            @"<\/?sup>|" +
            @"<\/?u>|" +
            @"<\/?uppercase>|" +
            @"<\/?voffset.*?>|" +
            @"<\/?width.*?>";

        // DWC-2021.3.18 upgrade: moved from old custom version of TMP to here to replace TMP_TextUtilities.GetParsedText
        public static string GetParsedText(string input)
        {
            return Regex.Replace(input, TAGS_REGEX, string.Empty);
        }

        private static string NormalizeUserName(string userName)
        {
            return _stripWhitespace.Replace(userName, "").ToLower();
        }

        public static void AddBindingsFromText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            
            var mentionBindings = GetMatchedBindings(text, MENTION_PATTERN);
            var hashtagBindings = GetMatchedBindings(text, HASHTAG_PATTERN);
            
            foreach (var binding in mentionBindings.Concat(hashtagBindings))
            {
                if (!NativeKeyboardManager.RichTextBindingEngine.TryGetBindingFromName(binding.name, out var _))
                {
                    NativeKeyboardManager.RichTextBindingEngine.AddBinding(binding);
                }
            }
        }
        
        private static IEnumerable<RichTextBindingData> GetMatchedBindings(string input, string pattern)
        {
            var matchesData = new List<RichTextBindingData>();
            
            if (string.IsNullOrWhiteSpace(input)) return matchesData;

            var findMatchesRegex = new Regex(pattern, RegexOptions.Compiled | RegexOptions.Multiline);
            var matches = findMatchesRegex.Matches(input);
            
            foreach (Match match in matches)
            {
                if (!match.Success) continue;
                
                if (!long.TryParse(match.Groups[1].Value, out var _)) continue;

                var tagName = match.Groups[2].Value;
                var richText = match.Value;
                matchesData.Add(new RichTextBindingData(tagName, richText));
            }

            return matchesData;
        }
    }
}