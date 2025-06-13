using System.Collections;
using System.Collections.Generic;
using Bridge.Services.UserProfile;
using Common;
using Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Common.Constants.ProfileLinks;

namespace UIManaging.Pages.UserProfile.Ui
{
    public class ProfileInfoPanel : MonoBehaviour
    {
        [SerializeField] private ProfileScrollablePanel _scrollablePanel;
        [SerializeField] private ProfileCoverPhotoAnimator _coverPhotoAnimator;
        [SerializeField] private RectTransform _coverPhoto;
        [SerializeField] private RectTransform _videoTabs;
        [Space]
        [SerializeField] private CanvasGroup _infoGroup;
        [SerializeField] private RectTransform _divider;
        [SerializeField] private TMP_Text _bioText;
        [SerializeField] private RectTransform _linksContainer;
        [Space]
        [SerializeField] private Button _youtubeButton;
        [SerializeField] private Button _instagramButton;
        [SerializeField] private Button _tikTokButton;

        private RectTransform _rectTransform;
        private RectTransform _bioTextTransform;
        private float _defaultHeight;
        private float _dividerHeight;
        private float _linksHeight;

        private float _defaultCoverOffset;
        private float _defaultVideoTabsOffset;

        private Coroutine _bioLayoutCoroutine;
        private Coroutine _bioFadeInCoroutine;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _rectTransform = GetComponent<RectTransform>();
            _bioTextTransform = _bioText.GetComponent<RectTransform>();

            _defaultHeight = _rectTransform.GetHeight();
            _dividerHeight = _divider.GetHeight();
            _linksHeight = _linksContainer.GetHeight();

            _defaultCoverOffset = _coverPhoto.GetBottom();
            _defaultVideoTabsOffset = _videoTabs.GetTop();

            _divider.SetActive(false);
            _bioText.SetActive(false);
            _linksContainer.SetActive(false);
            _infoGroup.alpha = 0f;
        }

        private void OnDestroy()
        {
            _youtubeButton.onClick.RemoveAllListeners();
            _instagramButton.onClick.RemoveAllListeners();
            _tikTokButton.onClick.RemoveAllListeners();

            CoroutineSource.Instance.SafeStopCoroutine(_bioLayoutCoroutine);
            CoroutineSource.Instance.SafeStopCoroutine(_bioFadeInCoroutine);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void ShowBio(Profile profile)
        {
            _bioLayoutCoroutine = CoroutineSource.Instance.StartCoroutine(SetBioAndUpdateLayout(profile));
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private static void InitLinkButton(IReadOnlyDictionary<string, string> profileLinks, string key, Button button)
        {
            if (profileLinks.TryGetValue(key, out var url) && !string.IsNullOrEmpty(url))
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => Application.OpenURL(url));
                button.SetActive(true);
            }
            else
            {
                button.onClick.RemoveAllListeners();
                button.SetActive(false);
            }
        }

        private IEnumerator SetBioAndUpdateLayout(Profile profile)
        {
            var profileBio = profile.Bio;
            var profileLinks = profile.BioLinks;

            var hasBio = !string.IsNullOrEmpty(profileBio);
            var hasLinks = profileLinks != null && profileLinks.Count > 0;

            var heightAdjustment = 0f;

            _divider.SetActive(false);
            _bioText.SetActive(false);
            _linksContainer.SetActive(false);

            if (hasBio || hasLinks)
            {
                _divider.SetActive(true);
                heightAdjustment += _dividerHeight;

                if (hasBio)
                {
                    _bioText.SetActive(true);
                    _bioText.text = profileBio;

                    yield return new WaitForEndOfFrame();

                    var preferredHeight = _bioText.preferredHeight;
                    heightAdjustment += preferredHeight;
                    _bioTextTransform.SetSizeY(preferredHeight);
                }

                if (hasLinks)
                {
                    heightAdjustment += _linksHeight;
                    _linksContainer.SetActive(true);

                    InitLinkButton(profileLinks, LINK_KEY_YOUTUBE, _youtubeButton);
                    InitLinkButton(profileLinks, LINK_KEY_INSTAGRAM, _instagramButton);
                    InitLinkButton(profileLinks, LINK_KEY_TIKTOK, _tikTokButton);
                }
            }

            var size = _rectTransform.GetSize();
            size.y = _defaultHeight + heightAdjustment;
            _rectTransform.SetSize(size);

            _scrollablePanel.TopYPosition = _scrollablePanel.DefaultTopYPosition + heightAdjustment;
            _coverPhotoAnimator.StartSizes[_rectTransform] = size;
            _coverPhoto.SetBottom(_defaultCoverOffset + heightAdjustment);
            _videoTabs.SetTop(_defaultVideoTabsOffset + heightAdjustment);

            _bioFadeInCoroutine = CoroutineSource.Instance.StartCoroutine(FadeIn());
        }

        private IEnumerator FadeIn()
        {
            yield return new WaitForEndOfFrame();

            while (_infoGroup.alpha < 1f)
            {
                _infoGroup.alpha += Time.deltaTime * 3;
                yield return null;
            }
        }
    }
}