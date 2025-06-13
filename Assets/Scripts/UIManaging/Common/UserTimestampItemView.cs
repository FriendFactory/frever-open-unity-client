using System;
using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Services.UserProfile;
using DG.Tweening;
using Extensions;
using JetBrains.Annotations;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.FollowersManagement;
using UIManaging.Pages.Common.Helpers;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Common
{
    public abstract class UserTimestampItemView<T> : BaseContextDataView<T> where T : UserTimestampItemModel
    {
        private const float LOADING_FADE_DURATION = 0.3f;

        [SerializeField] private RawImage _userThumbnail;
        [SerializeField] private TextMeshProUGUI _timeStampText;
        [SerializeField] private Button _thumbnailButton;
        [SerializeField] private CanvasGroup _loadingCircleCanvasGroup;
        [SerializeField] private GameObject _loadingCircle;
        [SerializeField] private Texture2D _defaultUserIcon;
        
        protected CancellationTokenSource CancellationSource;
        protected IBridge Bridge;
        protected PageManager PageManager;

        protected FollowersManager FollowersManager;
        private CharacterThumbnailsDownloader _characterThumbnailsDownloader;
        
        private Texture2D _cachedProfileTexture;
        private bool _skipDownloadingProfile;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected RawImage UserThumbnail => _userThumbnail;
        protected Texture2D DefaultUserIcon => _defaultUserIcon;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject, UsedImplicitly]
        private void Construct(IBridge bridge, PageManager pageManager, FollowersManager followersManager,
            CharacterThumbnailsDownloader characterThumbnailsDownloader)
        {
            Bridge = bridge;
            PageManager = pageManager;
            FollowersManager = followersManager;
            _characterThumbnailsDownloader = characterThumbnailsDownloader;
        }

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected Profile UserProfile { get; private set; }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void OnDisable()
        {
            CancelLoading();
        }

        protected override void OnDestroy()
        {
            CancelLoading(true);
            base.OnDestroy();
        }
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            CancellationSource?.CancelAndDispose();
            CancellationSource = new CancellationTokenSource();

            _skipDownloadingProfile = UserProfile?.MainGroupId == ContextData.GroupId;
            Setup();
        }

        protected override void BeforeCleanup()
        {
            CancelLoading();
            _userThumbnail.texture = _defaultUserIcon;
            _thumbnailButton.onClick.RemoveListener(PrefetchUser);
        }

        protected void PrefetchUser()
        {
            PrefetchDataForUser(ContextData.GroupId);
        }

        protected void PrefetchDataForUser(long groupId)
        {
            ContextData.OnMovingToProfileStart();
            GoToProfile(groupId);
        }

        private void CancelLoading(bool dispose = false)
        {
            if (dispose)
            {
                CancellationSource?.CancelAndDispose();
                CancellationSource = null;
            }
            else
            {
                CancellationSource?.Cancel();
            }
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SetupElapsedTimeText()
        {
            _timeStampText.text = ContextData.TimeStampText;
        }

        private void GoToProfile(long groupId)
        {
            var transitionArgs = new PageTransitionArgs
            {
                TransitionFinishedCallback = ContextData.OnMovingToProfileFinished,
                SaveCurrentPageToHistory = true
            };

            PageManager.MoveNext
            (
                PageId.UserProfile,
                new UserProfileArgs(groupId, null),
                transitionArgs
            );
        }

        private async void Setup()
        {
            SetupElapsedTimeText();
            ShowLoadingCircle();

            try
            {
                await LoadContextData();
            }
            catch (Exception e)
            {
                if (IsDestroyed || e is OperationCanceledException) return; //Do nothing
                throw;
            }
        }

        protected virtual void OnUserProfileDownloadSuccess()
        {
            DownloadUserProfileThumbnail(); 
            _thumbnailButton.onClick.AddListener(PrefetchUser);
            HideLoadingCircle();
        }
        
        protected virtual void OnUserProfileDownloadFailed()
        {
            HideLoadingCircle();
            _userThumbnail.texture = _defaultUserIcon;
        }
        
        private void OnProfileDownloadingSkipped()
        {
            HideLoadingCircle();
            _userThumbnail.texture = _cachedProfileTexture;
            _thumbnailButton.onClick.AddListener(PrefetchUser);
        }

        protected virtual async Task LoadContextData()
        {
            await LoadProfile();
        }

        private async Task LoadProfile()
        {
            if (_skipDownloadingProfile)
            {
                OnProfileDownloadingSkipped();
                return;
            }
            
            var result = await Bridge.GetProfile(ContextData.GroupId, CancellationSource.Token);
            
            CancellationSource?.Token.ThrowIfCancellationRequested();
            
            if (result.IsSuccess)
            {
                UserProfile = result.Profile;
                OnUserProfileDownloadSuccess();
            }
            else if (result.IsError)
            {
                OnUserProfileDownloadFailed();
            }
        }

        private void DownloadUserProfileThumbnail()
        {
            if(UserProfile?.MainCharacter == null) return;

            CancellationSource.Token.ThrowIfCancellationRequested();
            
            _characterThumbnailsDownloader
               .GetCharacterThumbnail(UserProfile.MainCharacter, Resolution._128x128, OnThumbnailLoaded, cancellationToken: CancellationSource.Token);
      
        }

        private void OnThumbnailLoaded(Texture2D thumbnail)
        {
            if (_userThumbnail.IsDestroyed()) return;

            _userThumbnail.texture = thumbnail;
            _cachedProfileTexture = thumbnail;
        }

        private void ShowLoadingCircle()
        {
            _loadingCircleCanvasGroup.alpha = 0f;
            _loadingCircle.SetActive(true);
            _loadingCircleCanvasGroup.DOKill();
            _loadingCircleCanvasGroup.DOFade(1f, LOADING_FADE_DURATION);
            _loadingCircleCanvasGroup.blocksRaycasts = true;
        }

        private void HideLoadingCircle()
        {
            _loadingCircleCanvasGroup.DOKill();
            _loadingCircleCanvasGroup.DOFade(0f, LOADING_FADE_DURATION)
                .OnComplete(() => _loadingCircle.SetActive(false));
            _loadingCircleCanvasGroup.blocksRaycasts = false;
        }
    }
}