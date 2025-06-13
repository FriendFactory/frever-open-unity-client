using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Abstract;
using Common;
using Extensions;
using Modules.SocialActions;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Common.Args.Views.Profile;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.SocialActions
{
    public sealed class SocialActionCard : BaseContextDataView<SocialActionCardModel>
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private AnimatedSkeletonBehaviour _animatedSkeleton;
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Image _background;
        [SerializeField] private List<TextMeshProLinkHandler> _linkHandlers;

        [Space] 
        [SerializeField] private Button _headerButton;
        [SerializeField] private UserPortrait _userPortrait;
        [SerializeField] private RawImage _thumbnail;
        [SerializeField] private GameObject _tempalteThumbnail;
        [SerializeField] private TMP_Text _header;
        [SerializeField] private TMP_Text _deescription;

        [Space] 
        [SerializeField] private TMP_Text _message;

        [Space] 
        [SerializeField] private Button _performActionButton;
        [SerializeField] private TMP_Text _performActionLabel;
        [SerializeField] private Image _performActionIcon;

        [Inject] private PageManager _pageManager;

        private CancellationTokenSource _cancellationTokenSource;
        private Coroutine _coroutine;

        private void OnEnable()
        {
            _canvasGroup.alpha = 1;
            _deleteButton.interactable = true;
            _deleteButton.onClick.AddListener(OnDeleteButtonClick);
            
            _performActionButton.interactable = true;
            _performActionButton.onClick.AddListener(OnPerformActionClick);

            _linkHandlers.ForEach(lh => lh.HyperlinkHandled += OnUserMentionClick);
            _headerButton.onClick.AddListener(OnHeaderClick);
            
            if (!IsInitialized && !_animatedSkeleton.IsPlaying)
            {
                _animatedSkeleton.Play();
            }
        }

        private void OnDisable()
        {
            _deleteButton.onClick.RemoveAllListeners();
            _performActionButton.onClick.RemoveAllListeners();
            _headerButton.onClick.RemoveAllListeners();
            
            _thumbnail.gameObject.SetActive(true);
            _tempalteThumbnail.SetActive(false);
            
            _linkHandlers.ForEach(lh => lh.HyperlinkHandled -= OnUserMentionClick);

            if (_coroutine != null) StopCoroutine(_coroutine);

            if (_animatedSkeleton.IsPlaying)
            {
                _animatedSkeleton.CleanUp();
            }
        }

        protected override void OnInitialized()
        {
            _animatedSkeleton.FadeOut();
            
            _background.sprite = ContextData.Background;
            _background.SetActive(true);

            _header.text = ContextData.Header;
            _deescription.text = ContextData.Description;

            _message.text = ContextData.Message;

            _performActionLabel.text = ContextData.PerformActionLabel;
            _performActionIcon.sprite = ContextData.PerformActionIcon;
            
            _cancellationTokenSource = new CancellationTokenSource();
            
            InitializeThumbnails();
        }

        protected override void BeforeCleanup()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.CancelAndDispose();
                _cancellationTokenSource = null;
            }
            
            _animatedSkeleton.CleanUp();
            _userPortrait.CleanUp();
        }

        private async void InitializeThumbnails()
        {
            if (_thumbnail.texture != null) return;

            if (ContextData.ThumbnailUrl is null && ContextData.ThumbnailProfile is null)
            {
                _thumbnail.gameObject.SetActive(false);
                _tempalteThumbnail.SetActive(true);
                return;
            }

            if (ContextData.ThumbnailProfile is null)
            {
                StartCoroutine(DownloadThumbnail());
                return;
            }

            await _userPortrait.InitializeAsync(ContextData.ThumbnailProfile, Resolution._128x128, _cancellationTokenSource.Token);
            _userPortrait.ShowContent();
        }

        private void OnDeleteButtonClick()
        {
            _deleteButton.interactable = false;
            _coroutine = StartCoroutine(FadeOutRoutine(OnFadeOutComplete));

            void OnFadeOutComplete()
            {
                ContextData.DeleteAction?.Invoke(ContextData.RecommendationId, ContextData.ActionId);
                CleanUp();
            }
        }

        private void OnPerformActionClick()
        {
            _performActionButton.interactable = !ContextData.MarkInstantlyAsDone;
            _coroutine = StartCoroutine(FadeOutRoutine(OnFadeOutComplete));

            void OnFadeOutComplete()
            {
                if (ContextData.MarkInstantlyAsDone)
                    ContextData.ActionCompleted?.Invoke(ContextData.RecommendationId, ContextData.ActionId);
                ContextData.SocialAction.Execute();
            }
        }

        private IEnumerator DownloadThumbnail()
        {
            var request = UnityWebRequestTexture.GetTexture(ContextData.ThumbnailUrl);
            yield return request.SendWebRequest();
            if (request.result is UnityWebRequest.Result.ConnectionError or UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError(request.error);
                yield break;
            }

            var texture = ((DownloadHandlerTexture)request.downloadHandler).texture;
            _thumbnail.texture = texture;
        }

        private void OnUserMentionClick(string linkId, string linkText)
        {
            var split = linkId.Split(':');

            if (split.Length == 0) return;

            if (!long.TryParse(split[1], out var groupId)) return;
            
            _pageManager.MoveNext(PageId.UserProfile, new UserProfileArgs(groupId, null));
        }

        private void OnHeaderClick()
        {
            ContextData.HeaderButtonClick?.Invoke();
        }

        // Convert to DoTween, for some reason cannot  find DOFade at the time of writing
        private IEnumerator FadeOutRoutine(Action onComplete)
        {
            while (_canvasGroup.alpha > 0)
            {
                _canvasGroup.alpha -= Time.deltaTime * 3;
                yield return new WaitForEndOfFrame();
            }
            
            onComplete?.Invoke();
        }
    }
}