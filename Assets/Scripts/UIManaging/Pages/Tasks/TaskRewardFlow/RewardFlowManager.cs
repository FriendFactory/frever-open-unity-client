using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Gamification;
using Bridge.Models.ClientServer.UserActivity;
using Bridge.Services.UserProfile;
using Common.UserBalance;
using Extensions;
using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using UIManaging.Pages.Common.Seasons;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks.RewardPopUp;
using UIManaging.Pages.Tasks.TaskRewardFlow;
using UIManaging.PopupSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Tasks
{
    public class RewardFlowManager : MonoBehaviour
    {
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private PopupManager _popupManager;

        [SerializeField] private FlyingRewardsAnimationController _flyingRewardsAnimationController;
        [SerializeField] private SeasonLevelInfoView _seasonLevelInfoView;
        [SerializeField] private UserBalanceView _userBalance;

        private FlowState _flowState = FlowState.Idle;
        private int _xpReward;
        private int _softCurrencyReward;
        private int _hardCurrencyReward;
        private Queue<SeasonLevel> UnlockedLevels;
        private UserBalanceArgs _userBalanceArgs;
        private SeasonLevelInfoAnimatedModel _seasonLevelModel;

        private int _currentSoftCurrency;
        private int _currentHardCurrency;
        private int _currentLevel;
        private int _currentXp;
        private int _newLevel;

        public Action<RewardFlowResult> FlowCompleted;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize(UserBalance balance, XpInfo xpInfo)
        {
            Initialize(balance.SoftCurrencyAmount, balance.HardCurrencyAmount, xpInfo.CurrentLevel.Level, xpInfo.Xp);
        }
        
        public void Initialize(int softCurrencyBeforeReward, int hardCurrencyBeforeReward, int levelBeforeReward, int xpBeforeReward)
        {
            _currentSoftCurrency = softCurrencyBeforeReward;
            _currentHardCurrency = hardCurrencyBeforeReward;
            _currentLevel = levelBeforeReward;
            _currentXp = xpBeforeReward;
            
            _userBalance.ContextData?.CleanUp();
            _userBalance.Initialize(InitialUserBalanceArgs());
        }
        
        public void StartAnimation(int xpReward, int softCurrencyReward, int hardCurrencyReward)
        {
            if (_flowState != FlowState.Idle)
            {
                HandleClaimDuringAnimation(xpReward, softCurrencyReward, hardCurrencyReward);
                return;
            }
                
            _xpReward = xpReward;
            _softCurrencyReward = softCurrencyReward;
            _hardCurrencyReward = hardCurrencyReward;
            

            if (XpAnimationPossible()) PrepareSeasonLevelData();
            _userBalanceArgs = GetAnimatedBalanceArgs();
            
            PlayFlyingRewardsAnimation();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private IUserBalanceModel InitialUserBalanceArgs()
        {
            return new StaticUserBalanceModel(_currentSoftCurrency, _currentHardCurrency);
        }

        private void PrepareSeasonLevelData()
        {
            var currentSeason = _dataFetcher.CurrentSeason;
            _seasonLevelModel = new SeasonLevelInfoAnimatedModel(_currentXp, _xpReward, currentSeason, _currentLevel);

            UnlockedLevels = GetUnlockedLevels();
            _newLevel = UnlockedLevels.Count == 0 ? _currentLevel : UnlockedLevels.ToList().Last().Level;
            
            if(UserLeveledUp()) SendLevelReachedEvent(_newLevel);
        }

        private void PlayFlyingRewardsAnimation()
        {
            _flowState = FlowState.FlyingRewards;
            
            var softCurrencyAnimation = _softCurrencyReward > 0;
            var hardCurrencyAnimation = _hardCurrencyReward > 0; 
            var xpAnimation = _dataFetcher.CurrentSeason != null && _xpReward > 0;
            
            _flyingRewardsAnimationController.Play(softCurrencyAnimation, hardCurrencyAnimation, xpAnimation);
            _flyingRewardsAnimationController.FirstElementReachedTarget += OnFirstFlyingRewardReachedTarget;
            _flyingRewardsAnimationController.LastElementReachedTarget += OnLastFlyingRewardReachedTarget;
        }

        private void OnFirstFlyingRewardReachedTarget()
        {
            _flowState = FlowState.XpBar;
            
            _flyingRewardsAnimationController.FirstElementReachedTarget -= OnFirstFlyingRewardReachedTarget;

            if (_userBalance != null && (_softCurrencyReward > 0 || _hardCurrencyReward > 0))
            {
                _userBalance.ContextData?.CleanUp();
                _userBalance.Initialize(new AnimatedUserBalanceModel(_userBalanceArgs));
            }
            
            if (!XpAnimationPossible()) return;

            AnimateExperienceRewardGain();
        }
        
        private void OnLastFlyingRewardReachedTarget()
        {
            _flyingRewardsAnimationController.LastElementReachedTarget -= OnLastFlyingRewardReachedTarget;

            if (!XpAnimationPossible()) CleanUp();
        }

        private bool XpAnimationPossible()
        {
            return _dataFetcher.CurrentSeason != null && _seasonLevelInfoView != null && _xpReward > 0;
        }

        private void AnimateExperienceRewardGain()
        {
            if (!XpAnimationPossible())
            {
                CleanUp();
                return;
            }
            
            _seasonLevelInfoView.SequenceFinished += OnUserLevelUpAnimationFinished;
            _seasonLevelInfoView.Initialize(_seasonLevelModel);
        }
        
        private void OnUserLevelUpAnimationFinished()
        {
            _seasonLevelInfoView.SequenceFinished -= OnUserLevelUpAnimationFinished;
            CleanUp();
        }

        private async void CleanUp()
        {
            _flowState = FlowState.Idle;
            await _localUser.RefreshUserInfoAsync();
            
            if (this.IsDestroyed()) return;
            
            _userBalance.ContextData?.CleanUp();

            FlowCompleted?.Invoke(new RewardFlowResult(_currentLevel, _newLevel));
            _userBalance.ContextData?.CleanUp();
            _userBalance.Initialize(new StaticUserBalanceModel(_localUser));
            _currentLevel = _newLevel;
        }

        private Queue<SeasonLevel> GetUnlockedLevels()
        {
            var seasonLevels = _dataFetcher.CurrentSeason.Levels;
            
            // Max level -> no level up
            if (_currentLevel >= seasonLevels.Length) return new Queue<SeasonLevel>();

            var xpAfterReward = _xpReward + _currentXp;
            var currentLevelInfo = seasonLevels[_currentLevel - 1];
            var nextLevel = seasonLevels[_currentLevel];

            // Not enough for level up -> no level up
            if (xpAfterReward < nextLevel.XpRequired) return new Queue<SeasonLevel>();
            
            var queue = new Queue<SeasonLevel>();
            queue.Enqueue(currentLevelInfo);

            // Working under assumptions that season levels array will stay sorted in the ascending order
            foreach (var level in seasonLevels)
            {
                // if we were already on this level skip
                if (level.Level <= _currentLevel) continue;
                // stop if we reached levels above level after task 
                if (level.XpRequired > xpAfterReward) break;
                
                queue.Enqueue(level);
            }
            
            return queue;
        }

        private bool UserLeveledUp()
        {
            return _currentLevel != _newLevel;
        }

        // Move to season rewards flow manager
        private void SendLevelReachedEvent(int newLevel)
        {
            var levelUpMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.USER_LEVEL_REACHED] = newLevel,
                [AmplitudeEventConstants.EventProperties.SEASON_ID] = _dataFetcher.CurrentSeason.Id
            };
            
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.USER_LEVEL_UP, levelUpMetaData);
        }

        private UserBalanceArgs GetAnimatedBalanceArgs()
        {
            var softTo = _currentSoftCurrency + _softCurrencyReward;
            var hardTo = _currentHardCurrency + _hardCurrencyReward;

            return new UserBalanceArgs(0.0f, 0.5f, 
                                       _currentSoftCurrency, softTo, 
                                       _currentHardCurrency, hardTo);
        }

        private void HandleClaimDuringAnimation(int xpReward, int softCurrency, int hardCurrency)
        {
            switch (_flowState)
            {
                case FlowState.FlyingRewards:
                    HandleClaimDuringFlyingRewards(xpReward, softCurrency, hardCurrency);        
                    
                    break;
                case FlowState.XpBar :
                    HandleClaimDuringXpBar(xpReward, softCurrency, hardCurrency);
                    
                    return;
            }
        }

        private void HandleClaimDuringFlyingRewards(int xpReward, int softCurrency, int hardCurrency)
        {
            _xpReward += xpReward;
            PrepareSeasonLevelData();

            if (_softCurrencyReward == 0 && softCurrency != 0)
            {
                _flyingRewardsAnimationController.Play(true, false, false);
            }
            _softCurrencyReward += softCurrency;
            if (_hardCurrencyReward == 0 && hardCurrency != 0)
            {
                _flyingRewardsAnimationController.Play(false, true, false);
            }
            _hardCurrencyReward += hardCurrency;
            
            _userBalanceArgs = GetAnimatedBalanceArgs();
            _userBalance.Initialize(new AnimatedUserBalanceModel(_userBalanceArgs));
        }

        private void HandleClaimDuringXpBar(int xpReward, int softCurrency, int hardCurrency)
        {
            _xpReward += xpReward;
            _seasonLevelInfoView.UpdateXp(xpReward);
            PrepareSeasonLevelData();

            _softCurrencyReward += softCurrency;
            _hardCurrencyReward += hardCurrency;
        }

        private enum FlowState
        {
            Idle = 0,
            FlyingRewards = 1,
            XpBar = 2,
        }
    }
}