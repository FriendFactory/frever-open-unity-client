using System.Text.RegularExpressions;
using Bridge.Models.VideoServer;
using UIManaging.Common.InputFields;
using UIManaging.Common.Hashtags;

namespace UIManaging.Pages.SharingPage.Ui
{
    internal sealed class DescriptionHashtagsHandler
    {
        private const string HASHTAG_REGEX = "(?<!<mark=)#\\w{1,25}";
        private const string HASHTAG_TAG_REGEX = "<\\/?[A-Za-z]+?=\"htg\">";
        private const string HASHTAG_LINK_REGEX = "<link=\"htg\"><b=\"htg\">(\\s*(.+?)\\s*)</b=\"htg\"></link=\"htg\">";
        private const string HASHTAG_LINK_PATTERN = "<link=\"htg\"><b=\"htg\">$&</b=\"htg\"></link=\"htg\">";

        private readonly IInputFieldAdapter _inputFieldAdapter;
        private readonly HashtagsPanel _hashtagsPanel;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public DescriptionHashtagsHandler(IInputFieldAdapter inputFieldAdapter, HashtagsPanel hashtagsPanel)
        {
            _inputFieldAdapter = inputFieldAdapter;
            _hashtagsPanel = hashtagsPanel;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void HandleHashtags()
        {
            UpdateHashtags();
            HandleSuggestions();
        }

        public void ReplaceHashtag(HashtagInfo info)
        {
            var text = _inputFieldAdapter.Text;
            var stringPosition = _inputFieldAdapter.StringPosition;
            var isMatch = false;

            var matches = Regex.Matches(text, HASHTAG_LINK_REGEX);
            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var matchStart = match.Index;
                    var matchEnd = match.Index + match.Length;

                    if (matchStart + 1 < stringPosition && stringPosition < matchEnd + 1)
                    {
                        ReplaceHashtag(text, info, match.Groups[1], matchEnd);

                        isMatch = true;
                        break;
                    }
                }
            }

            if (!isMatch && stringPosition > 0 && text[stringPosition - 1] == '#')
            {
                InsertHashtag(text, info, stringPosition);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateHashtags()
        {
            var text = _inputFieldAdapter.Text;
            var caretPosition = _inputFieldAdapter.CaretPosition;

            var isModified = false;

            if (Regex.IsMatch(text, HASHTAG_LINK_REGEX))
            {
                text = Regex.Replace(text, HASHTAG_TAG_REGEX, string.Empty);
                isModified = true;
            }

            if (Regex.IsMatch(text, HASHTAG_REGEX))
            {
                text = Regex.Replace(text, HASHTAG_REGEX, HASHTAG_LINK_PATTERN);
                isModified = true;
            }

            if (!isModified) return;

            _inputFieldAdapter.SetTextWithoutNotify(text);
            _inputFieldAdapter.StringPosition = _inputFieldAdapter.GetStringIndexFromCaretPosition(caretPosition);
        }

        private void HandleSuggestions()
        {
            var text = _inputFieldAdapter.Text;
            var stringPosition = _inputFieldAdapter.StringPosition;
            var matches = Regex.Matches(text, HASHTAG_LINK_REGEX);
            var isMatch = false;

            if (matches.Count > 0)
            {
                foreach (Match match in matches)
                {
                    var matchStart = match.Index;
                    var matchEnd = match.Index + match.Length;

                    if (matchStart + 1 < stringPosition && stringPosition < matchEnd + 1)
                    {
                        var hashtag = match.Groups[1].Value.Substring(1);
                        _hashtagsPanel.Show(hashtag);
                        isMatch = true;
                        break;
                    }
                }
            }

            if (!isMatch && stringPosition > 0 && text[stringPosition - 1] == '#')
            {
                isMatch = true;
                _hashtagsPanel.Show(string.Empty);
            }

            if (!isMatch)
            {
                _hashtagsPanel.Hide();
            }
        }

        private void ReplaceHashtag(string text, HashtagInfo info, Group matchGroup, int matchEnd)
        {
            var groupStart = matchGroup.Index;
            var groupLenght = matchGroup.Length;

            _inputFieldAdapter.Text = text
                                     .Insert(matchEnd, " ")
                                     .Remove(groupStart, groupLenght)
                                     .Insert(groupStart, $"#{info.Name}");

            _inputFieldAdapter.StringPosition = matchEnd + info.Name.Length - groupLenght + 2;
            _inputFieldAdapter.ActivateInputField();
        }

        private void InsertHashtag(string text, HashtagInfo info, int stringPosition)
        {
            _inputFieldAdapter.Text = text.Insert(stringPosition, $"{info.Name} ");
            _inputFieldAdapter.StringPosition += info.Name.Length + 1;
            _inputFieldAdapter.ActivateInputField();
        }
    }
}