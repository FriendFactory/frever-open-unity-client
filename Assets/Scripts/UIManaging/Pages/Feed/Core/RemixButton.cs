using System;
using System.Linq;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Extensions;
using Modules.Amplitude;
using Modules.FeaturesOpening;
using Modules.LevelManaging.Editing.Templates;
using Modules.VideoStreaming.Feed;
using Navigation.Core;
using UIManaging.Common;
using UIManaging.Localization;
using UIManaging.Pages.Common.Helpers;
using UIManaging.Pages.PreRemixPage.Ui;
using UIManaging.PopupSystem;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.Feed.Ui.Feed
{
    internal class RemixButton : BaseContextDataButton<FeedVideoModel>
    {
        [SerializeField] private Image _creatorThumbnail;

        [Inject] private PageManager _pageManager;
        [Inject] private RemixLevelSetup _remixSetup;
        [Inject] private IAppFeaturesManager _iAppFeaturesManager; 
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] protected AmplitudeManager AmplitudeManager;
        [Inject] private IBridge _bridge;
        [Inject] private ITemplateProvider _templateProvider;
        [Inject] private MusicDownloadHelper _musicDownloadHelper;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private FeedLocalization _localization;
        
        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action ButtonClicked;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            if (_creatorThumbnail != null)
            {
                SetupThumbnail();
            }
        }

        private async void SetupThumbnail()
        {
            if (_creatorThumbnail.sprite != null)
            {
                Destroy(_creatorThumbnail.sprite);
            }
            _creatorThumbnail.SetActive(false);
            var originalCreator = ContextData.Video.OriginalCreator;
            if (!ContextData.Video.RemixedFromVideoId.HasValue ||  originalCreator == null || originalCreator.Id == ContextData.Video.GroupId) return;

            var thumbnail = originalCreator.MainCharacterFiles.First(x => x.Resolution == Resolution._128x128);
            var thumbnailResp = await _bridge.GetCharacterThumbnailAsync(originalCreator.MainCharacterId.Value, thumbnail);
            if (thumbnailResp.IsError) return;

            _creatorThumbnail.SetActive(true);
            _creatorThumbnail.sprite = (thumbnailResp.Object as Texture2D).ToSprite();
            _creatorThumbnail.preserveAspect = true;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            _button.onClick.AddListener(OnClick);
            _button.interactable = true;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _button.onClick.RemoveListener(OnClick);
        }
        
        protected void StartRemixSetup()
        {
            _remixSetup.Setup(ContextData.Video, () => _button.interactable = true);
        }

        protected virtual async void OnClick()
        {
            _button.interactable = false;

            var isMusicAvailable = await CheckMusicAvailable();

            _button.interactable = true;

            if (!isMusicAvailable) return;
            
            if (!_iAppFeaturesManager.IsCreationNewLevelAllowed)
            {
                _popupManagerHelper.ShowLockedLevelCreationFeaturePopup(_iAppFeaturesManager.ChallengesRemainedForEnablingNewLevelCreation);
                return;
            }
            
            AmplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.CLICK_REMIX);

            var isOriginVideo = !ContextData.Video.RemixedFromVideoId.HasValue;
            var hasRemixes = ContextData.Video.KPI.Remixes > 0;

            if (!isOriginVideo || hasRemixes)
            {
                var preRemixArgs = new PreRemixPageArgs(ContextData.Video);
                _pageManager.MoveNext(PageId.PreRemixPage, preRemixArgs);
            }
            else
            {
                StartRemixSetup();
            }

            InvokeButtonClicked();
        }

        protected void InvokeButtonClicked()
        {
            _button.interactable = false;
            ButtonClicked?.Invoke();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void Reset()
        {
            _button = GetComponentInChildren<Button>();
        }
        
        private async Task<bool> CheckMusicAvailable()
        {
            if (!ContextData.Video.LevelId.HasValue) return true;
            
            var levelResult = await _bridge.GetLevel(ContextData.Video.LevelId.Value);
            if (levelResult.IsError)
            {
                var errorCode = levelResult.GetErrorCodeFromErrorMessage();
                if (errorCode.Contains("InaccessibleAssets"))
                {
                    _snackBarHelper.ShowFailSnackBar(_localization.RemixFailedInaccessibleAssetsAssetsError);
                }
                
                return false;
            }

            if (levelResult.IsRequestCanceled) return false;

            var level = levelResult.Model?.Level;
            if (level == null)
            {
                return false;
            }

            var results = await Task.WhenAll(level.Event.Select(
                                            ev => _musicDownloadHelper.CheckIfMusicAvailableAsync(ev.MusicController?.ExternalTrackId)));

            var isAvailable = results.All(songAvailable => songAvailable);
            
            return isAvailable;
        }
        
    }
}