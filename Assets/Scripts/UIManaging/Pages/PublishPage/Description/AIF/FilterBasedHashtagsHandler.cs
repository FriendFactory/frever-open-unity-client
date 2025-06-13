using System.Collections;
using AdvancedInputFieldPlugin;
using Bridge.Models.VideoServer;
using UIManaging.Common.Hashtags;
using UnityEngine;

namespace UIManaging.Pages.SharingPage.Ui
{
    public class FilterBasedHashtagsHandler: DescriptionHandlerBase<HashtagInfo>
    {
        [SerializeField] private HashtagsPanel _hashtagsPanel;

        public bool IsActive => _hashtagsPanel.IsActive;
        
        protected override char StartWith => '#';
        protected override bool IsTerminating(char @char) => @char == '\n';

        private void OnEnable()
        {
            _hashtagsPanel.HashtagItemClicked += OnItemSelected;
        }

        private void OnDisable()
        {
            _hashtagsPanel.HashtagItemClicked -= OnItemSelected;
        }

        protected override RichTextBindingData GetRichTextBindingData(HashtagInfo item)
        {
            var tagName = $"{StartWith}{item.Name}";
            var richText = string.Format(HashtagsPanel.HASHTAG_TAG, tagName, item.Id);

            return new RichTextBindingData(tagName, richText);
        }

        protected override void OnItemSelected(HashtagInfo info)
        {
            if (info == null)
            {
                StartCoroutine(RemoveLastChar());
                return;
            }
            
            base.OnItemSelected(info);
        }

        public override void Process(Occurrence occurrence)
        {
            base.Process(occurrence);

            var start = occurrence.wordStartIndex + 1;
            var end = occurrence.wordEndIndex;
            var filter = occurrence.text.Substring(start, end - start).ToLower();
            
            _hashtagsPanel.Show(filter);
        }

        public override void Hide()
        {
            if (!InputField.Selected)
            {
                InputField.ManualSelect();
            }
            _hashtagsPanel.Hide();
        }
        
        private IEnumerator RemoveLastChar()
        {
            yield return null;

            if (string.IsNullOrEmpty(InputField.Text)) yield break;
            
            var text = InputField.Text.Remove(InputField.Text.Length - 1);
            InputField.Text = text;
            InputField.CaretPosition -= 1; 
            InputField.ManualSelect();
        }
    }
}