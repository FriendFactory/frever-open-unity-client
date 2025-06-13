using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge;
using Bridge.Models.ClientServer.Gamification;
using Common;
using Common.UserBalance;
using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common.Rewards;
using UIManaging.Localization;
using UIManaging.Pages.Common.Seasons;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.SeasonPage.Likes;
using UIManaging.Pages.Tasks;
using UIManaging.Popups.Store;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;
using UnityEngine;
using UnityEngine.UI;
using Zenject;
using static Navigation.Args.SeasonPageArgs.Tab;
using Resolution = Bridge.Models.Common.Files.Resolution;

namespace UIManaging.Pages.SeasonPage
{
    internal sealed class SeasonPage : GenericPage<SeasonPageArgs>
    {
        [SerializeField] private SeasonPassPurchaseHelper _seasonPassPurchaseHelper;
        [SerializeField] private RewardFlowManager _rewardFlowManager;
        [SerializeField] private SeasonThumbnailBackground _background;
        [SerializeField] private SeasonLevelInfoView _seasonLevelInfoView;
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _infoButton;
        
        [Header("Toggles")]
        [SerializeField] private Toggle _rewardsToggle;
        [SerializeField] private SeasonRewardsContent _rewardsContent;
        [Space]
        [SerializeField] private Toggle _likesToggle;
        [SerializeField] private GameObject _likesIndicator;
        [SerializeField] private SeasonLikesContent _likesContent;
        
        [Header("Popups")] 
        [SerializeField] private SeasonRewardPreviewPopup _previewRewardPrefab;
        [SerializeField] private UserBalanceView _userBalanceView;
        [SerializeField] private StoreButton _storeButton;

        [Inject] private PopupManager _popupManager;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private RewardEventModel _pageModel;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private IBridge _bridge;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private SeasonRewardPreviewModel.Factory _rewardPreviewModelFactory;
        [Inject] private SeasonPageLocalization _localization;

        private PageManager _pageManager;
        private SeasonRewardPreviewPopup _previewPopup;
        private bool _isRewardPreviewInstantiated;

        private CancellationTokenSource _tokenSource;

        private SeasonPageArgs.Tab _currentTab;
        private bool _isUserInfoUpdated;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.SeasonInfo;
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _infoButton.onClick.AddListener(OnInfoButtonClicked);
            _rewardsToggle.onValueChanged.AddListener(OnRewardsToggleValueChanged);
            _likesToggle.onValueChanged.AddListener(OnLikesToggleValueChanged);
            
            _pageModel.AssetRewardClaimed += OnAssetRewardClaimed;
            _pageModel.CurrencyRewardClaimed += OnCurrencyRewardClaimed;
            _pageModel.PreviewRequested += OnPreviewRequested;
            _pageModel.LockedRewardClicked += OnLockedRewardClicked;
            _seasonPassPurchaseHelper.PremiumPassPurchased += _rewardsContent.ReloadData;
            
            UpdateTabIndicators();
            _likesContent.SwitchToRewardTabRequested += SwitchToRewardsTab;
            _storeButton.UserBalanceUpdated += OnStoreUserBalanceUpdated;
            _localUser.UserBalanceUpdated += OnUserBalanceUpdated;
        }

        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackButtonClicked);
            _infoButton.onClick.RemoveListener(OnInfoButtonClicked);
            _rewardsToggle.onValueChanged.RemoveListener(OnRewardsToggleValueChanged);
            _likesToggle.onValueChanged.RemoveListener(OnLikesToggleValueChanged);
            
            _pageModel.AssetRewardClaimed -= OnAssetRewardClaimed;
            _pageModel.CurrencyRewardClaimed -= OnCurrencyRewardClaimed;
            _pageModel.PreviewRequested -= OnPreviewRequested;
            _pageModel.LockedRewardClicked -= OnLockedRewardClicked;
            _seasonPassPurchaseHelper.PremiumPassPurchased -= _rewardsContent.ReloadData;
            
            _likesContent.SwitchToRewardTabRequested -= SwitchToRewardsTab;
            _storeButton.UserBalanceUpdated -= OnStoreUserBalanceUpdated;
            _localUser.UserBalanceUpdated -= OnUserBalanceUpdated;
        }

        //---------------------------------------------------------------------
        // Page
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            _pageManager = pageManager;
        }
        
        protected override async void OnDisplayStart(SeasonPageArgs args)
        {
            base.OnDisplayStart(args);
            
            _tokenSource = new CancellationTokenSource();

            var userBalanceModel = new StaticUserBalanceModel(_localUser);
            _userBalanceView.ContextData?.CleanUp();
            _userBalanceView.Initialize(userBalanceModel);
            UpdateBalance();
            ShowSeasonStartPopUp();
            
            _seasonLevelInfoView.Initialize(new SeasonLevelInfoStaticModel(_localUser));
            LoadBackgroundAsync(_tokenSource.Token);

            _currentTab = args.StartingTab;

            switch (_currentTab)
            {
                case Rewards:
                    _rewardsContent.ShowSkeleton();
                    _likesContent.Hide();
                    _rewardsToggle.SetIsOnWithoutNotify(true);
                    break;
                case Quests:
                    _likesContent.ShowSkeleton();
                    _rewardsContent.Hide();
                    _likesToggle.SetIsOnWithoutNotify(true);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var result = await _userData.RefreshUserInfoAsync();
            if (!result) return;
            if (IsDestroyed) return;

            _isUserInfoUpdated = true;

            switch (_currentTab)
            {
                case Rewards:
                    _rewardsContent.Show();
                    _likesContent.Hide();
                    break;
                case Quests:
                    _likesContent.Show(args.StartQuestId);
                    _rewardsContent.Hide();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        protected override void OnHidingBegin(Action onComplete)
        {
            _rewardsContent.Hide();
            _likesContent.Hide();
            _tokenSource?.Cancel();
            _userBalanceView.ContextData?.CleanUp();
            _userBalanceView.CleanUp();
            
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateBalance()
        {
            _localUser.UpdateBalance(_tokenSource.Token);
        }

        private void OnRewardsToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                _currentTab = Rewards;

                if (_isUserInfoUpdated)
                {
                    _rewardsContent.Show();
                }
                else
                {
                    _rewardsContent.ShowSkeleton();
                }
            }
            else
            {
                _rewardsContent.Hide();
            }
        }
        
        private void OnLikesToggleValueChanged(bool isOn)
        {
            if (isOn)
            {
                _currentTab = Quests;

                if (_isUserInfoUpdated)
                {
                    _likesContent.Show();
                }
                else
                {
                    _likesContent.ShowSkeleton();
                }
            }
            else
            {
                _likesContent.Hide();
            }
        }

        private void OnBackButtonClicked()
        {
            if (OpenPageArgs.MoveBack == null)
            {
                _pageManager.MoveBack();
            }
            else
            {
                OpenPageArgs.MoveBack.Invoke();
            }
        }

        private void OnInfoButtonClicked()
        {
            _popupManager.SetupPopup(new SeasonInfoPopupConfiguration());
            _popupManager.ShowPopup(PopupType.SeasonInfo);
        }
        
        private async void OnAssetRewardClaimed(long rewardId, Sprite thumbnail, Action<bool> callback)
        {
            _userData.RefreshUserInfo();
            
            var result = await _bridge.ClaimLevelReward(rewardId);

            callback?.Invoke(result.IsSuccess);
            
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
            }

            if (!result.IsSuccess)
            {
                return;
            }

            var reward = GetReward(rewardId);
            
            var assetClaimedPopupConfiguration = new AssetClaimedPopupConfiguration
            {
                Thumbnail = thumbnail,
                Level = reward.Level ?? 0,
                Title = string.Format(_localization.AssetClaimedTier, reward.Level),
            };
            
            _rewardsContent.SetRewardAsClaimed(rewardId);
            
            _popupManager.SetupPopup(assetClaimedPopupConfiguration);
            _popupManager.ShowPopup(PopupType.AssetClaimedPopup);

            _dataFetcher.ResetData();
            _dataFetcher.FetchUserAssets();
            
            UpdateTabIndicators();
        }

        private void OnPreviewRequested(long rewardId)
        {
            PreviewReward(rewardId, false);
        }
        
        private void PreviewReward(long rewardId, bool isLocked) 
        {
            if (!_isRewardPreviewInstantiated)
            {
                _previewPopup = Instantiate(_previewRewardPrefab, transform);
                _previewPopup.transform.SetAsLastSibling();
                _isRewardPreviewInstantiated = true;
            }

            var reward = GetReward(rewardId);
            var model = _rewardPreviewModelFactory.Create();
            
            model.Reward = reward;
            model.TargetLevel = GetTargetLevel(rewardId);
            model.CurrentLevel = _localUser.CurrentLevel;
            model.IsLocked = isLocked;
                
            _previewPopup.Show(model);
            SendPreviewRewardEvent(reward);
        }
        
        private async void OnCurrencyRewardClaimed(long rewardId, Action<bool> callback)
        {
            _rewardFlowManager.Initialize(_localUser.UserBalance, _localUser.LevelingProgress.Xp);
            
            var result = await _bridge.ClaimLevelReward(rewardId);

            callback?.Invoke(result.IsSuccess);
            
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }
            
            var reward = GetReward(rewardId);

            _rewardsContent.SetRewardAsClaimed(rewardId);
            
            _rewardFlowManager.StartAnimation(0, reward.SoftCurrency ?? 0, reward.HardCurrency ?? 0);
            UpdateTabIndicators();
        }
        
        private async void LoadBackgroundAsync(CancellationToken token)
        {
            await _background.InitializeAsync(Resolution._512x512, token);

            if (_tokenSource.IsCancellationRequested)
            {
                return;
            }
            
            _background.ShowContent();
        }

        private void UpdateTabIndicators()
        {
            _likesIndicator.SetActive(false);
            
            var quests = _dataFetcher.CurrentSeason.Quests.OrderBy(quest => quest.Value).ToArray();

            foreach (var quest in quests)
            {
                if (_userData.LevelingProgress.LikesReceivedThisSeason < quest.Value) break;

                var rewards = quest.Rewards?.Length > 0 ? quest.Rewards : null;
                if (rewards is null) break;

                var reward = rewards.First();
                var claimed = reward != null && (_userData.LevelingProgress.RewardClaimed?.Contains(reward.Id) ?? false);
                if (claimed) break;

                _likesIndicator.SetActive(true);
            }
        }
        
        private void OnLockedRewardClicked(long rewardId)
        {
            PreviewReward(rewardId, true);
        }

        private void SwitchToRewardsTab()
        {
            _rewardsContent.ReloadData();
            _rewardsToggle.isOn = true;
        }

        private SeasonReward GetReward(long rewardId)
        {
            foreach (var level in _dataFetcher.CurrentSeason.Levels)
            {
                var reward = level.Rewards?.FirstOrDefault(item => item.Id == rewardId);
                if (reward != null) return reward;
            }

            return null;
        }

        private int GetTargetLevel(long rewardId)
        {
            return _dataFetcher.CurrentSeason.Levels
                        .TakeWhile(level => level.Rewards.All(item => item.Id != rewardId))
                        .Count() + 1;
        }
                
        private void SendPreviewRewardEvent(SeasonReward reward)
        {
            var metaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.SEASON_REWARD_ID] = reward.Id,
                [AmplitudeEventConstants.EventProperties.SEASON_ID] = reward.SeasonId,
                [AmplitudeEventConstants.EventProperties.REWARD_LEVEL] = reward.Level,
                [AmplitudeEventConstants.EventProperties.SEASON_TRACK_TYPE] = reward.IsPremium ? "Premium" : "Free",
                [AmplitudeEventConstants.EventProperties.ASSET_ID] = reward.Asset?.Id
            };
            
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.PREVIEW_SEASON_REWARD, metaData);
        }

        private void OnStoreUserBalanceUpdated()
        {
            _rewardFlowManager.Initialize(_localUser.UserBalance, _localUser.LevelingProgress.Xp);
        }

        private void ShowSeasonStartPopUp()
        {
            var currenSeasonId = _dataFetcher.CurrentSeason?.Id.ToString();
            var lastShownSeasonKey = PlayerPrefs.GetString(Constants.Onboarding.SEEN_SEASON_POPUP_IDENTIFIER, null);

            if (currenSeasonId == null || lastShownSeasonKey == currenSeasonId) return;

            PlayerPrefs.SetString(Constants.Onboarding.SEEN_SEASON_POPUP_IDENTIFIER,
                                  _dataFetcher.CurrentSeason?.Id.ToString());

            _popupManager.PushPopupToQueue(new SeasonPopupConfiguration());
        }
        
        private void OnUserBalanceUpdated()
        {
            _seasonLevelInfoView.ContextData.UpdateForNextCycle();
            _seasonLevelInfoView.Initialize(_seasonLevelInfoView.ContextData);
            _rewardsContent.RefreshActiveData();
        }
    }
}