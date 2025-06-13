using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Services.UserProfile;
using Common.Abstract;
using UIManaging.Localization;
using UIManaging.Pages.RatingFeed.Amplitude;
using UIManaging.Pages.RatingFeed.Rating;
using UIManaging.Pages.RatingFeed.Reward;
using UIManaging.Pages.RatingFeed.Social;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.SwipeToFollow;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.RatingFeed
{
    internal sealed class RatingFeedView: BaseContextView<RatingFeedViewModel>
    {
        private const int MIN_RATING_SCORE = 4;

        [SerializeField] private RatingVideosListView _ratingVideosListView;
        [SerializeField] private RatingFeedProgressPanel _progressPanel;
        [SerializeField] private Button _skipButton;
        [Header("Reward Panel")]
        [SerializeField] private RatingFeedRaterRewardView _raterRewardView;
        [Header("Favorites Panel")]
        [SerializeField] private RatingFeedFavoritesView _favoritesView;

        [Inject] private PopupManager _popupManager;
        [Inject] private RatingFeedPageLocalization _loc;
        [Inject] private IBridge _bridge;

        private List<Profile> _topRatedProfiles;
        private Task<List<Profile>> _topRatedProfilesTask;

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action SkipRequested;
        public event Action MoveNextRequested;
        
        //---------------------------------------------------------------------
        // Unity Callbacks 
        //---------------------------------------------------------------------

        private void OnApplicationQuit()
        {
            ContextData.CancelRating(VideoRatingCancellationReason.ApplicationQuit);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public async void ShowRewardPanel()
        {
            try
            {
                _popupManager.ClosePopupByType(PopupType.DialogDarkV3);
                await _raterRewardView.FadeInAsync();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInitialized()
        {
            _ratingVideosListView.Initialize(ContextData.RatingVideosListModel);
            _progressPanel.Initialize(ContextData.RatingFeedProgress);

            _skipButton.onClick.AddListener(OnSkipRequested);
            
            _raterRewardView.Initialize(ContextData.Level);
            _raterRewardView.Shown += OnRewardShown;
            _raterRewardView.RewardClaimed += OnRewardClaimed;
        }
        
        protected override void BeforeCleanUp()
        {
            _skipButton.onClick.RemoveListener(OnSkipRequested);
            
            _raterRewardView.CleanUp();

            _raterRewardView.Shown -= OnRewardShown;
            _raterRewardView.RewardClaimed -= OnRewardClaimed;

            _popupManager.ClosePopupByType(PopupType.DialogDarkV3);
            
            _ratingVideosListView.CleanUp();
            _progressPanel.CleanUp();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void ShowSkipRatingPopup()
        {
            var confirmPopupConfiguration = new DialogDarkPopupConfiguration
            {
                PopupType = PopupType.DialogDarkV3,
                Title = _loc.SkipRatingDialogTitle,
                Description = _loc.SkipRatingDialogDesc,
                YesButtonText = _loc.SkipRatingDialogYesButton,
                YesButtonSetTextColorRed = true,
                NoButtonText = _loc.SkipRatingDialogNoButton,
                OnYes = OnSkipConfirmed,
            };

            _popupManager.SetupPopup(confirmPopupConfiguration);
            _popupManager.ShowPopup(confirmPopupConfiguration.PopupType);

            void OnSkipConfirmed()
            {
                ContextData.CancelRating(VideoRatingCancellationReason.Skipped);
                SkipRequested?.Invoke();
            }
        }
        
        private void OnSkipRequested()
        {
            ShowSkipRatingPopup();
        }

        private async void OnRewardShown()
        {
            try
            {
                _topRatedProfilesTask = GetTopRatedProfiles();
                _topRatedProfiles = await _topRatedProfilesTask;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async void OnRewardClaimed()
        {
            try
            {
                _topRatedProfiles = await _topRatedProfilesTask;
                if (_topRatedProfiles.Count > 0)
                {
                    _favoritesView.Initialize(_topRatedProfiles);
                    _favoritesView.SkipFavorites += OnSkipRequested;
                    _favoritesView.ShowFavorites += OnShowFavoritesRequested;

                    var fadeOutTask = _raterRewardView.FadeOutAsync();
                    var fadeInTask = _favoritesView.FadeInAsync();

                    await Task.WhenAll(fadeOutTask, fadeInTask);
                }
                else
                {
                    MoveToNextPage();
                }

            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private void OnShowFavoritesRequested()
        {
            var popupConfig = new SwipeToFollowPopupConfiguration(_topRatedProfiles, OnClose);
            _popupManager.SetupPopup(popupConfig);
            _popupManager.ShowPopup(popupConfig.PopupType);

            void OnClose(object obj)
            {
                MoveToNextPage();
            }
        }

        private async Task<List<Profile>> GetTopRatedProfiles()
        {
            var groupIds = ContextData.RatingVideos
                               .Where(video => video.Rating.Score >= MIN_RATING_SCORE && !video.Video.IsFollower)
                               .Select(video => video.Video.Owner.Id)
                               .Distinct().ToList();

            var tasks = groupIds.Select(id => _bridge.GetProfile(id)).ToList();
            await Task.WhenAll(tasks);

            var profiles = from task in tasks where task.Result.IsSuccess select task.Result.Profile;
            var filteredProfiles = profiles.Where(profile => !string.IsNullOrWhiteSpace(profile.Bio)).ToList();

            return filteredProfiles.ToList();
        }

        private void MoveToNextPage()
        {
            MoveNextRequested?.Invoke();
        }
    }
}