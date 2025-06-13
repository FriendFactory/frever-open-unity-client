using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Common.Hyperlinks
{
    public static class HyperlinkParser
    {
        public const string MENTION_STYLE_DEFAULT = "Mention";
        public const string MENTION_STYLE_PINK = "MentionPink";
        
        private const string SIMPLE_LINK_FORMAT = "<link=\"{0}\">{1}</link>";
        private const string MENTION_LINK_FORMAT = "<link=\"mention:{0}\"><style=\"{2}\">@{1}</style></link>";
        private const string HASHTAG_REGEX = "#\\w{1,25}";
        private const string HASHTAG_LINK_FORMAT = "<link=\"hashtag:{0}\"><b>{1}</b></link>";
        private const string MENTION_REGEX = "@([0-9]+)";

        public static string ParseHyperlinks(string text, IDictionary<long, string> mentions, IDictionary<string, long> hashtagToIdMapping, string mentionStyle = MENTION_STYLE_PINK)
        {
            text = LinkHashtags(text, hashtagToIdMapping);

            if (mentions != null)
            {
                text = LinkMentions(text, mentions, mentionStyle);
            }

            return text;
        }

        public static string ParseMentionHyperLink(string name, long id)
        {
            return string.Format(SIMPLE_LINK_FORMAT, id, name);
        }

        private static string LinkHashtags(string text, IDictionary<string, long> hashtagToIdMapping)
        {
            if (hashtagToIdMapping == null) return text;

            string HashtagMatchEvaluator(Match match)
            {
                if (!hashtagToIdMapping.TryGetValue(match.Value.Substring(1), out var hashTagId))
                {
                    return match.Value;
                }

                return string.Format(HASHTAG_LINK_FORMAT, hashTagId, match.Value);
            }

            return Regex.Replace(text, HASHTAG_REGEX, HashtagMatchEvaluator);
        }

        private static string LinkMentions(string text, IDictionary<long, string> mentions, string mentionStyle)
        {
            if (mentions == null) return text;

            string MentionMatchEvaluator(Match match)
            {
                if (!long.TryParse(match.Groups[1].Value, out var groupId))
                {
                    return match.Value;
                }
                
                if (!mentions.TryGetValue(groupId, out var username))
                {
                    return match.Value;
                }

                return string.Format(MENTION_LINK_FORMAT, match.Groups[1].Value, username, mentionStyle);
            }

            return Regex.Replace(text, MENTION_REGEX, MentionMatchEvaluator);
        }
    }
}