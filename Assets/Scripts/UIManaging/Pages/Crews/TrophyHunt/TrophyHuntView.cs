using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Crews;
using Bridge.Models.ClientServer.UserActivity;
using Common;
using DG.Tweening;
using Extensions;
using Extensions.DateTime;
using ModestTree;
using Modules.Crew;
using Sirenix.OdinInspector;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Common.Rewards;
using UIManaging.EnhancedScrollerComponents;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Crews.TrophyHunt.Models;
using UIManaging.Pages.Tasks;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.TrophyHunt
{
    public class TrophyHuntView : MonoBehaviour
    {
        private const int MEMBERLIST_COUNT = 4;

        [SerializeField] private TrophyHuntRewardsListView _rewardsListView;
        [SerializeField] private TrophyHuntMemberListView _memberListView;

        [SerializeField] private Button _showRewardsButton;
        [SerializeField] private Button _rewardsRowButton;
        [SerializeField] private Button _hideRewardsButton;
        [SerializeField] private Button _showInfoButton;
        [SerializeField] private Button _hdeInfoButton;
        [SerializeField] private Button _viewAllButton;
        [SerializeField] private Button _crewTopListButton;
        [SerializeField] private TMP_Text _weekText;
        [SerializeField] private TMP_Text _timeLeftText;
        [SerializeField] private TMP_Text _crewScoreText;
        [SerializeField] private TMP_Text _announcementText;
        [FormerlySerializedAs("_rewardAnimationFlowManager")] [SerializeField] private RewardFlowManager _rewardFlowManager;

        [SerializeField] private GameObject _rewardsProgressView;
        [SerializeField] private GameObject _aboutView;
        
        [SerializeField] private GameObject _rewardAvailableBadge;
        [SerializeField] private Image _rewardAvailableBadgeMask;

        [SerializeField] private TrophyHuntRewardListItem[] _rewardRow;

        [SerializeField] private RectTransform _announcementRect;
        [SerializeField] private Button _announcementCloseButton;

        [SerializeField] private RawImage _competitionBackground;
        [SerializeField] private AnimatedSkeletonBehaviour _skeletonBehaviour;
        
        [Inject] private IBridge _bridge;
        [Inject] private CrewService _crewService;
        [Inject] private RewardEventModel _rewardEventModel;
        [Inject] private LocalUserDataHolder _userData;
        [Inject] private PopupManager _popupManager;
        [Inject] private CrewPageLocalization _localization;
       
        private CrewModel _crewData;
        private List<CrewRewardWrapper> _sortedRewardList;

        private void Awake()
        {
            _showInfoButton.onClick.AddListener(OnShowInfoButton);
            _hdeInfoButton.onClick.AddListener(OnHideInfoButton);
            
            _showRewardsButton.onClick.AddListener(OnShowRewardsButton);
            _rewardsRowButton.onClick.AddListener(OnShowRewardsButton);
            
            _viewAllButton.onClick.AddListener(OnViewAllButtonClicked);
            _announcementCloseButton.onClick.AddListener(HideAnnouncement);
            
            _crewTopListButton.onClick.AddListener(OnCrewTopButton);
        }

        private void OnEnable()
        {
            SubscribeRewardEvents();
        }
        
        private void OnDisable()
        {
            UnsubscribeRewardEvents();
        }

        public void Show()
        {
            _competitionBackground.texture = _crewService.GetTrophyHuntBackground();
            gameObject.SetActive(true);
            Init();
        }

        private async void Init()
        {
            _skeletonBehaviour.SetActive(true);
            _skeletonBehaviour.Play();
            
            await LoadCrewData();
            
            _skeletonBehaviour.FadeOut();

            _timeLeftText.text = string.Format(_localization.TrophyHuntTimeLeftFormat, _crewData.Competition.EndDate.GetFormattedTimeLeft());
            _crewScoreText.text = _crewData.Competition.TrophyScore.ToString();
            _weekText.text = string.Format(_localization.TrophyHuntWeekFormat ,_crewData.Competition.WeekNumber);
            
            InitRewards();
            InitMemberList();
            InitAnnouncement();

            _rewardFlowManager.Initialize(_userData.UserBalance, _userData.LevelingProgress.Xp);
        }

        private void InitAnnouncement()
        {
            var showAnnouncement = 
                PlayerPrefs.GetInt(Constants.Crew.LAST_ANNOUNCEMENT_CLOSED_WEEK, 0) != _crewData.Competition.WeekNumber;
            _announcementRect.SetActive(showAnnouncement);
        }

        private void InitRewardRow()
        {
            foreach (var rewardView in _rewardRow)
            {
                rewardView.SetActive(false);
            }

            const int rewardsInRow = 5;
            var selectedRewards = new List<CrewRewardWrapper>(_sortedRewardList);
            var maxVisibleIndex = selectedRewards.Count - rewardsInRow;

            var rangeFirst = selectedRewards.FindIndex(reward => reward.Reward.RequiredTrophyScore <= _crewData.Competition.TrophyScore 
                                                              && (_crewData.Competition.ClaimedRewardIds == null 
                                                               || !_crewData.Competition.ClaimedRewardIds.Contains(reward.Id)));
            
            if (rangeFirst < 0)
            {
                rangeFirst =
                    selectedRewards.FindIndex(reward => reward.Reward.RequiredTrophyScore > _crewData.Competition.TrophyScore);
            }

            rangeFirst = rangeFirst < 0
                ? maxVisibleIndex 
                : Mathf.Min(rangeFirst, maxVisibleIndex);
            
            selectedRewards = selectedRewards.GetRange(rangeFirst, rewardsInRow);

            for (var i = 0; i < selectedRewards.Count; i++)
            {
                _rewardRow[i].SetActive(true);
                _rewardRow[i].Initialize(selectedRewards[i]);
            }
        }

        private async void InitMemberList()
        {
            var result = await _bridge.GetCrewMembersTopList(_crewData.Id, default);
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }

            var members = result.Models;

            var currentUser = 
                members.FirstOrDefault(user => user.Group.Id == _userData.UserProfile.MainGroupId);

            var membersList = members.Take(MEMBERLIST_COUNT).ToArray();

            if (!membersList.Contains(currentUser))
            {
                membersList[membersList.Length - 1] = currentUser;
            }
                
            _memberListView.Initialize(membersList.Select(member => new TrophyHuntMemberListModel
            {
                Member = member,
                LeaderboardRank = members.IndexOf(member),
                IsLocalUser = member.Group.Id == _userData.GroupId
            }).ToArray());
        }

        private async Task LoadCrewData()
        {
            await _userData.DownloadProfile();
            var crewData = await _bridge.GetCrew(_userData.UserProfile.CrewProfile.Id, default);

            _crewData = crewData.Model;
        }

        private void InitRewards()
        {
            _sortedRewardList = _crewData.Competition.Rewards.Select(reward =>
            {
                RewardState state;

                if (_crewData.Competition.ClaimedRewardIds != null
                 && _crewData.Competition.ClaimedRewardIds.Contains(reward.Id))
                {
                    state = RewardState.Claimed;
                }
                else if (_crewData.Competition.TrophyScore < reward.RequiredTrophyScore)
                {
                    state = RewardState.Locked;
                }
                else
                {
                    state = RewardState.Available;
                }

                return new CrewRewardWrapper(reward, state);
            }).ToList();
            
            _sortedRewardList.Sort(
                (reward1, reward2) => reward1.Reward.RequiredTrophyScore - reward2.Reward.RequiredTrophyScore);
            
            var rewardListModel = new BaseEnhancedScroller<CrewRewardWrapper>(_sortedRewardList);
            _rewardsListView.TrophyScore = _crewData.Competition.TrophyScore;
            _rewardsListView.Initialize(rewardListModel);
            
            RefreshRewards();
        }

        private void SubscribeRewardEvents()
        {
            _rewardEventModel.AssetRewardClaimed += OnAssetRewardClaimed;
            _rewardEventModel.LootboxRewardClaimed += OnLootboxRewardClaimed;
            _rewardEventModel.CurrencyRewardClaimed += OnCurrencyRewardClaimed;
            _rewardEventModel.PreviewRequested += OnPreviewRequested;
            _rewardEventModel.LockedRewardClicked += OnLockedRewardClicked;
        }

        private void UnsubscribeRewardEvents()
        {
            _rewardEventModel.AssetRewardClaimed -= OnAssetRewardClaimed;
            _rewardEventModel.LootboxRewardClaimed -= OnLootboxRewardClaimed;
            _rewardEventModel.CurrencyRewardClaimed -= OnCurrencyRewardClaimed;
            _rewardEventModel.PreviewRequested -= OnPreviewRequested;
            _rewardEventModel.LockedRewardClicked -= OnLockedRewardClicked;
        }

        private async void OnCurrencyRewardClaimed(long rewardId, Action<bool> callback)
        {
            var result = await _bridge.ClaimTrophyHuntReward(rewardId);

            callback?.Invoke(result.IsSuccess);
            
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }

            var reward = GetReward(rewardId);
            
            PlayRewardFlowAnimation(reward.SoftCurrency, reward.HardCurrency);
            
            _userData.UserBalance.SoftCurrencyAmount += reward.SoftCurrency.GetValueOrDefault();
            _userData.UserBalance.HardCurrencyAmount += reward.HardCurrency.GetValueOrDefault();
            SetRewardAsClaimed(rewardId);
            
            await LoadCrewData();
            RefreshRewards();
        }

        private async void OnAssetRewardClaimed(long rewardId, Sprite thumbnail, Action<bool> callback)
        {
            _userData.RefreshUserInfo();
            
            var result = await _bridge.ClaimTrophyHuntReward(rewardId);

            callback?.Invoke(result.IsSuccess);
            
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }
            
            var assetClaimedPopupConfiguration = new AssetClaimedPopupConfiguration
            {
                Title = _localization.TrophyHuntRewardClaimedSnackbarMessage,
                Thumbnail = thumbnail,
            };
            
            _popupManager.SetupPopup(assetClaimedPopupConfiguration);
            _popupManager.ShowPopup(PopupType.AssetClaimedPopup);
            
            SetRewardAsClaimed(rewardId);
            
            await LoadCrewData();
            RefreshRewards();
        }

        private void RefreshRewards()
        {
            UpdateTabIndicators();
            InitRewardRow();
        }

        private void SetRewardAsClaimed(long rewardId)
        {
            var reward = _rewardsListView.ContextData.Items.FirstOrDefault(item => item.Id == rewardId);
            if (reward != null) reward.State = RewardState.Claimed;
        }

        private async void OnLootboxRewardClaimed(long rewardId, Sprite lootboxSprite, Action<bool> callback)
        {
            _userData.RefreshUserInfo();
            
            var result = await _bridge.ClaimTrophyHuntReward(rewardId);
            
            callback?.Invoke(result.IsSuccess);
            
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }
                 
            SetRewardAsClaimed(rewardId);
            
            await LoadCrewData();
            RefreshRewards();
            
            var reward = GetReward(rewardId);
            
            var lootboxClaimedPopupConfiguration = new LootboxClaimedPopupConfiguration
            {
                PopupType = PopupType.LootBoxClaimedPopup,
                PossibleRewards = result.LootBoxAssets,
                Reward = result.Asset,
                LootboxTitle = reward.LootBox.Title,
                Thumbnail = lootboxSprite
            };

            _popupManager.SetupPopup(lootboxClaimedPopupConfiguration);
            _popupManager.ShowPopup(PopupType.LootBoxClaimedPopup);
        }

        [Button]
        private void ShowTestLootboxPopup()
        {
            var lootboxClaimedPopupConfiguration = new LootboxClaimedPopupConfiguration()
            {
                PopupType = PopupType.LootBoxClaimedPopup,
                PossibleRewards = new LootBoxAsset[10],
            };
            
            _popupManager.SetupPopup(lootboxClaimedPopupConfiguration);
            _popupManager.ShowPopup(PopupType.LootBoxClaimedPopup);
        }
        
        private void OnPreviewRequested(long rewardId)
        {
            
        }
        
        private void OnLockedRewardClicked(long rewardId)
        {
            
        }

        private void OnShowRewardsButton()
        {
            _rewardsProgressView.gameObject.SetActive(true);
        }

        private void OnShowInfoButton()
        {
            _aboutView.gameObject.SetActive(true);
        }

        private void OnHideInfoButton()
        {
            _aboutView.gameObject.SetActive(false);
        }
        
        private CrewReward GetReward(long rewardId)
        {
            return _crewData.Competition.Rewards.FirstOrDefault(item => item.Id == rewardId);
        }

        private void UpdateTabIndicators()
        {
            var rewardAvailable = IsRewardAvailable();
            _rewardAvailableBadge.SetActive(rewardAvailable);
            _rewardAvailableBadgeMask.enabled = rewardAvailable;
        }

        private bool IsRewardAvailable()
        {
            return _crewData.Competition.Rewards.Any(reward => reward.RequiredTrophyScore <= _crewData.Competition.TrophyScore
                                                             && (_crewData.Competition.ClaimedRewardIds == null ||
                                                                 !_crewData.Competition.ClaimedRewardIds.Contains(reward.Id)));
        }
        
        private void PlayRewardFlowAnimation(int? softCurrency, int? hardCurrency)
        {
            if (softCurrency.GetValueOrDefault() > 0 || hardCurrency.GetValueOrDefault() > 0)
            {
                _rewardFlowManager.StartAnimation(0, 
                                                           softCurrency.GetValueOrDefault(),
                                                           hardCurrency.GetValueOrDefault());
            }
        }

        private void OnViewAllButtonClicked()
        {
            var blockedMembers = _crewData.MembersCount - (int)_crewData.Members?.Length;
            var config = new ViewAllCrewMembersPopupConfiguration(_crewData.Id, _userData.GroupId, _crewData.Members.Length, blockedMembers);
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(PopupType.ViewAllCrewMembers, true);
        }
        
        private void OnCrewTopButton()
        {
            _popupManager.PushPopupToQueue(new CrewTopPopupConfiguration
            {
                PopupType = PopupType.CrewTopList,
                CompetitionTime = _crewData.Competition.EndDate,
                TimeLeft = _crewData.Competition.EndDate.GetFormattedTimeLeft()
            });
        }

        private void HideAnnouncement()
        {
            _announcementRect.DOAnchorPosY(_announcementRect.GetHeight(), 0.2f);
            PlayerPrefs.SetInt(Constants.Crew.LAST_ANNOUNCEMENT_CLOSED_WEEK, _crewData.Competition.WeekNumber);
        }
    }
}