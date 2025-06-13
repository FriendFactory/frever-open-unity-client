using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Bridge.Models.VideoServer;
using Common.Hyperlinks;
using DG.Tweening;
using Extensions;
using TMPro;
using UIManaging.Common.InputFields;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.VideoDetails
{
    public sealed class FeedVideoDescription : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI descriptionText;
        [SerializeField] private LayoutElement descriptionTextLayoutElement;
        [SerializeField] private Button expandDescriptionButton;
        [SerializeField] private float _defaultDescriptionHeight = 86.5f;
        [SerializeField] private bool _expandable = true;

        private bool _isExpanded;
        private string _defaultText;
        private string _ellipsizedText;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Init(Video video)
        {
            PrepareDescriptionText(video);
            SubscribeToEvents();
        }


        public void SimplifyForOnboarding(bool enable)
        {
            expandDescriptionButton.interactable = !enable;
        }

        public void BeforeCleanup()
        {
            UnsubscribeFromEvents();
            _isExpanded = false;
            descriptionTextLayoutElement.DOKill();
            descriptionTextLayoutElement.preferredHeight = _defaultDescriptionHeight;
            descriptionText.gameObject.SetActive(false);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SubscribeToEvents()
        {
            expandDescriptionButton.onClick.AddListener(OnExpandDescriptionButton);
        }

        private void UnsubscribeFromEvents()
        {
            expandDescriptionButton.onClick.RemoveListener(OnExpandDescriptionButton);
        }

        private async void PrepareDescriptionText(Video video)
        {
            _defaultText = video.Description;

            var noDescription = string.IsNullOrEmpty(_defaultText);
            descriptionText.SetActive(!noDescription);
            if (noDescription) return;

            var mentions = video.Mentions;
            var mentionsMapping = mentions?.ToDictionary
                (
                    mention => mention.GroupId,
                    mention => AdvancedInputFieldUtils.GetParsedText(mention.GroupNickname)
                );

            var hashtags = video.Hashtags;
            var hashtagsMapping = hashtags?.ToDictionary
                (
                    hashtag => hashtag.Name,
                    hashtag => hashtag.Id
                );

            _defaultText = HyperlinkParser.ParseHyperlinks(_defaultText, mentionsMapping, hashtagsMapping, HyperlinkParser.MENTION_STYLE_DEFAULT);


            descriptionText.transform.parent.SetActive(true);
            descriptionText.gameObject.SetActive(true);
            descriptionText.text = _defaultText;

            await Task.Yield();
            await Task.Yield();

            var preferredTextHeight = descriptionText.preferredHeight;
            var isTextTruncated = descriptionText.preferredHeight > _defaultDescriptionHeight;

            expandDescriptionButton.enabled = isTextTruncated;
            
            if (!isTextTruncated)
            {
                descriptionTextLayoutElement.preferredHeight = preferredTextHeight;
                return;
            }

            descriptionTextLayoutElement.preferredHeight = _defaultDescriptionHeight;
            HandleEllipsizedLink();
        }

        private void HandleEllipsizedLink()
        {
            _ellipsizedText = _defaultText;
            var lastIndex = GetLastEllipsizedCharacterIndex();
            var ellipsizedSubstring = _defaultText.Substring(0, lastIndex);

            if (lastIndex < _ellipsizedText.Length && 
                Regex.Matches(ellipsizedSubstring, "<link>").Count
             != Regex.Matches(ellipsizedSubstring, "</link>").Count)
            {
                _ellipsizedText = _ellipsizedText.Insert(lastIndex, "</style></link=ellipsized>");
            }

            descriptionText.text = _ellipsizedText;
        }

        private void OnExpandDescriptionButton()
        {
            if (!_expandable) return;
            
            _isExpanded = !_isExpanded;
            descriptionText.text = _isExpanded ? _defaultText : _ellipsizedText;

            descriptionTextLayoutElement.DOKill();
            descriptionTextLayoutElement.DOPreferredSize(
                _isExpanded
                    ? new Vector2(-1f, descriptionText.preferredHeight)
                    : new Vector2(-1f, _defaultDescriptionHeight), 0.3f);
        }

        private int GetLastEllipsizedCharacterIndex()
        {
            var textInfo = descriptionText.textInfo;
            var lastVisibleChar = textInfo.lineInfo[1].lastVisibleCharacterIndex;
            var lastIndex = textInfo.characterInfo[lastVisibleChar].index;
            
            for (var i = 0; i < textInfo.characterCount; i++)
            {
                if (textInfo.characterInfo[i].index >= lastVisibleChar)
                {
                    lastIndex = i;
                    break;
                }
            }
            
            return lastIndex;
        }
    }
}