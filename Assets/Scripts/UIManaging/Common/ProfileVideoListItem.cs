using System;
using System.Collections.Generic;
using System.Threading;
using Bridge.Models.VideoServer;
using Bridge;
using Common;
using Common.BridgeAdapter;
using Extensions;
using Models;
using Navigation.Args;
using TMPro;
using UIManaging.Common.Buttons;
using UIManaging.Localization;
using UIManaging.Pages.Common.VideoRating;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Common
{
    public sealed class ProfileVideoListItem : VideoListItem
    {
        private const string DATE_FORMAT = "dd/MM HH:mm";
        private const int WIN_PLACE_TO_SHOW_MAX = 3;

        [SerializeField] private TextMeshProUGUI _likesAmountText;
        [SerializeField] private TextMeshProUGUI _draftsAmountText;
        [SerializeField] private TextMeshProUGUI _createdDateText;
        [SerializeField] private GameObject _createdDateParentGameObject;
        [SerializeField] private GameObject _likesParentGameObject;
        [SerializeField] private GameObject _draftsParentGameObject;
        [SerializeField] private LevelPreviewButton _levelPreviewButton;
        [SerializeField] private LevelEditButton _levelEditButton;
        [SerializeField] private ScoreView _scoreView;
        
        [SerializeField] private LevelThumbnail _levelThumbnail;
        [SerializeField] private GameObject _galleryVideoIconGameObject;
        [SerializeField] private List<AccessObjects> _accessObjects;

        [Header("Rewards")]
        [SerializeField] private VideoRatingBadge _rewardBadge;
        [SerializeField] private GameObject _rewardWaitingOverlay;

        [Inject] private ILevelService _levelService;
        [Inject] private IBridge _bridge;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private ProfileLocalization _localization;
        [Inject] private VideoRatingStatusModel _videoRatingStatusModel;
        
        private CancellationTokenSource _profileCancellationSource;
        private bool _isRewardUIAvailable;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _isRewardUIAvailable = _rewardBadge != null && _rewardWaitingOverlay != null;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _profileCancellationSource?.Cancel();
        }


        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            var args = ContextData;
            _likesParentGameObject.SetActive(args.ShowLikes);
            _draftsParentGameObject.SetActive(args.ShowDrafts);
            _createdDateParentGameObject.SetActive(args.ShowCreationDate);

            var showGalleryVideoIcon = args.Video?.LevelId == null && args.Level == null;
            _galleryVideoIconGameObject.SetActive(showGalleryVideoIcon);

            if (args.ShowCreationDate)
            {
                _createdDateText.text = args.Level.CreatedTime.ToString(DATE_FORMAT);
            }

            InitializeScore(args);

            RefreshButtons();

            if (args.ShowDrafts)
            {
                SetupDraftCount();
            }
            
            _likesAmountText.text = args.Likes.ToString();
            
            UpdatePrivacyLabels(args.Video?.Access ?? VideoAccess.Public);
            UpdateRewardBadgeAndOverlay();
            
            base.OnInitialized();
        }

        protected override void RefreshThumbnail()
        {
            var args = ContextData;
            var hasVideoModel = args.Video != null;

            _videoThumbnail.gameObject.SetActive(hasVideoModel);
            _levelThumbnail.gameObject.SetActive(!hasVideoModel);

            if (hasVideoModel)
            {
                SwitchContentIsNotAvailableOverlay(false);   
                _videoThumbnail.Initialize(ContextData);
                _levelThumbnail.CleanUp();
            }
            else
            {
                SwitchContentIsNotAvailableOverlay(false);
                _levelThumbnail.Initialize(args.Level);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void InitializeScore(BaseLevelItemArgs args)
        {
            if (_scoreView == null)
            {
                #if UNITY_EDITOR
                if (args.ShowScore)
                {
                    UnityEngine.Debug.LogWarning($"Requested to show score on prefab without score view. Prefab instance name: {gameObject.name}");
                }
                #endif
                return;
            }

            var showScore = args.ShowScore && ContextData.Video.BattleResult != null
                                           && ContextData.Video.BattleResult.Place <= WIN_PLACE_TO_SHOW_MAX
                                           && ContextData.Video.BattleResult.Place > 0;

            _scoreView.SetActive(showScore);
            if (!showScore) return;
            _scoreView.Initialize(ContextData.Video.BattleResult);
        }

        private async void SetupDraftCount()
        {
            _profileCancellationSource = new CancellationTokenSource();
            var profile = await _bridge.GetMyProfile(_profileCancellationSource.Token);

            if (!profile.IsSuccess)
            {
                return;
            }

            var draftsAmount = profile.Profile.KPI.TotalDraftsCount;
            var draftsText = draftsAmount <= 1
                ? _localization.DraftsTextFormat
                : _localization.DraftsTextPluralFormat;
            _draftsAmountText.text = string.Format(draftsText, draftsAmount);
        }

        private void UpdatePrivacyLabels(VideoAccess access)
        {
            var isLocalUser = _bridge?.Profile.GroupId == (ContextData.Video?.GroupId ?? ContextData.Level?.GroupId);

            foreach (var accessObjects in _accessObjects)
            {
                foreach (var target in accessObjects.targets)
                {
                    target.SetActive(accessObjects.access == access && isLocalUser);
                }
            }
        }

        private void RefreshButtons()
        {
            var args = ContextData;

            _levelPreviewButton.gameObject.SetActive(args.ShowPreviewButton);
            _levelEditButton.gameObject.SetActive(args.ShowEditButton);

            if (args.ShowPreviewButton) _levelPreviewButton.Setup(DownloadFullLevelData);
            if (args.ShowEditButton) _levelEditButton.Setup(DownloadFullLevelData);

            _button.gameObject.SetActive(args.IsInteractable);
        }

        private async void DownloadFullLevelData()
        {
            var result = await _levelService.GetLevelAsync(ContextData.Level.Id);

            if (result.IsSuccess)
            {
                OnLevelDataLoaded(result.Level);
            }
            else if (result.ErrorMessage.Contains(Constants.ErrorMessage.ASSET_INACCESSIBLE_IDENTIFIER))
            {
                _snackBarHelper.ShowAssetInaccessibleSnackBar();
            }
        }

        private void OnLevelDataLoaded(Level levelData)
        {
            _levelPreviewButton.SetLevelData(levelData);
            _levelEditButton.SetLevelData(levelData);
        }

        private void UpdateRewardBadgeAndOverlay()
        {
            if (!_isRewardUIAvailable) return;

            _rewardWaitingOverlay.SetActive(false);
            _rewardBadge.Hide();

            var result = ContextData.Video?.RatingResult;
            if (result == null)
            {
                if (!_videoRatingStatusModel.IsCompleted || _videoRatingStatusModel.LevelId != ContextData.Video?.LevelId) return;
                
                _rewardWaitingOverlay.SetActive(true);
                return;
            }

            if (result.IsCompleted)
            {
                _rewardBadge.Show(result.Rating);
            }
            else
            {
                _rewardWaitingOverlay.SetActive(true);
            }
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        [Serializable]
        private struct AccessObjects
        {
            public VideoAccess access;
            public List<GameObject> targets;
        }
    }
}