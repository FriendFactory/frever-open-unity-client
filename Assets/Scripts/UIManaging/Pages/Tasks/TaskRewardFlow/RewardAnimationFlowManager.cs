/*
using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Gamification;
using Bridge.Models.ClientServer.UserActivity;
using Common.UserBalance;
using Extensions;
using Modules.Amplitude;
using Modules.AssetsStoraging.Core;
using Sirenix.OdinInspector;
using UIManaging.Pages.Common.Seasons;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks.RewardPopUp;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Tasks
{
    public sealed class RewardAnimationFlowManager : MonoBehaviour
    {
        [Inject] private LocalUserDataHolder _dataHolder;
        [Inject] private IDataFetcher _dataFetcher;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private PopupManager _popupManager;

        [SerializeField] private FlyingRewardsAnimationController _flyingRewardsAnimationController;
        [SerializeField] private SeasonLevelInfoView _seasonLevelInfoView;
        [SerializeField] private UserBalanceView _userBalance;

        private int _xpReward;
        private int _softCurrencyReward;
        private int _hardCurrencyReward;
        private XpInfo _xpInfo;
        private Queue<SeasonLevel> _unlockedLevels;
        private bool _transitionRequested;
        private bool _backwards;
        private UserBalanceArgs _userBalanceArgs;
        private SeasonLevelInfoAnimatedModel _seasonLevelModel;
        private bool _isPreset;
        private int _levelBeforeTask;

        public event Action<bool> FlowCompleted;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void PresetAnimation(int xpReward, int softCurrencyReward, int hardCurrencyReward, bool backwards = false)
        {
            if (_isPreset)
            {
                return;
            }
            
            _isPreset = true;
            
            _backwards = backwards;
            _xpInfo = _dataHolder.LevelingProgress.Xp;
            _xpReward = xpReward;
            _softCurrencyReward = softCurrencyReward;
            _hardCurrencyReward = hardCurrencyReward;
            _userBalanceArgs = GetAnimatedBalanceArgs(backwards);
            _levelBeforeTask = _seasonLevelInfoView?.Model.Level ?? 1;
            
            _userBalance.ContextData?.CleanUp();
            _userBalance.Initialize(new AnimatedUserBalanceModel(GetStaticBalanceArgs()));
        }
        
        public async void StartAnimation(int xpReward, int softCurrencyReward, int hardCurrencyReward, bool backwards = false, bool preset = true)
        {
            await _dataHolder.RefreshUserInfoAsync();

            _isPreset = (_isPreset && preset);
            PresetAnimation(xpReward, softCurrencyReward, hardCurrencyReward, true);
            _backwards = backwards;
            _isPreset = false;
            
            _transitionRequested = false;



            if (_dataFetcher.CurrentSeason != null)
            {
                _unlockedLevels = GetUnlockedLevels();
                var xpBeforeTask = _xpInfo.Xp - _xpReward;
                _levelBeforeTask = GetLevelBeforeTask(xpBeforeTask).Level;
                var currentSeason = _dataFetcher.CurrentSeason;
                _seasonLevelModel =
                    new SeasonLevelInfoAnimatedModel(_xpInfo, _xpReward, currentSeason, _levelBeforeTask);
            }

            SetState(TaskRewardFlowState.FlyingRewards);
        }
        
        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void SetState(TaskRewardFlowState flowState)
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            
            switch (flowState)
            {
                case TaskRewardFlowState.Idle:
                    OnFlowCompleted();

                    break;

                case TaskRewardFlowState.FlyingRewards:
                    PlayFlyingRewardsAnimation();

                    break;

                case TaskRewardFlowState.AnimateXpBar:
                    AnimateExperienceRewardGain();

                    break;

                case TaskRewardFlowState.RewardAnimationsDone:
                    OnExperienceRewardAnimationDone();

                    break;

                case TaskRewardFlowState.OpenLevelUpPopup:
                    OpenLevelUpPopup();

                    break;
            }
        }

        private void PlayFlyingRewardsAnimation()
        {
            var softCurrencyAnimation = _softCurrencyReward > 0;
            var hardCurrencyAnimation = _hardCurrencyReward > 0; 
            var xpAnimation = _dataFetcher.CurrentSeason != null && _xpReward > 0;
            
            _flyingRewardsAnimationController.Play(softCurrencyAnimation, hardCurrencyAnimation, xpAnimation);
            _flyingRewardsAnimationController.FirstElementReachedTarget += OnFirstFlyingRewardReachedTarget;
            _flyingRewardsAnimationController.LastElementReachedTarget += OnLastFlyingRewardReachedTarget;
        }

        private void OnFirstFlyingRewardReachedTarget()
        {
            _flyingRewardsAnimationController.FirstElementReachedTarget -= OnFirstFlyingRewardReachedTarget;

            if (_userBalance != null && (_softCurrencyReward > 0 || _hardCurrencyReward > 0))
            {
                _userBalance.ContextData?.CleanUp();
                _userBalance.Initialize(new AnimatedUserBalanceModel(_userBalanceArgs));
            }
            
            if (_dataFetcher.CurrentSeason == null || _seasonLevelInfoView == null || _xpReward == 0)
            {
                return;
            }
            
            SetState(TaskRewardFlowState.AnimateXpBar);
        }
        
        private void OnLastFlyingRewardReachedTarget()
        {
            _flyingRewardsAnimationController.LastElementReachedTarget -= OnLastFlyingRewardReachedTarget;

            if (_dataFetcher.CurrentSeason == null || _seasonLevelInfoView == null || _xpReward == 0)
            {
                SetState(TaskRewardFlowState.Idle);
            }
        }

        private void AnimateExperienceRewardGain()
        {

            if (_seasonLevelInfoView != null && _xpReward > 0)
            {
                _seasonLevelInfoView.SequenceFinished += OnUserLevelUpAnimationFinished;
                _seasonLevelInfoView.Initialize(_seasonLevelModel);
            }
            else
            {
                SetState(TaskRewardFlowState.RewardAnimationsDone);
            }
        }
        
        private void OnUserLevelUpAnimationFinished()
        {
            _seasonLevelInfoView.SequenceFinished -= OnUserLevelUpAnimationFinished;
            SetState(TaskRewardFlowState.RewardAnimationsDone);
        }

        private void OnExperienceRewardAnimationDone()
        {
            if (!UserLeveledUp())
            {
                SetState(TaskRewardFlowState.Idle);
                return;
            }

            SetState(TaskRewardFlowState.OpenLevelUpPopup);
        }

        private void OpenLevelUpPopup()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }
            
            var previousLevel = _unlockedLevels.Dequeue();
            var newLevel = _unlockedLevels.ToArray().Last();
            var config = new LevelUpPopupConfiguration(previousLevel.Level, _unlockedLevels, newLevel, OnLevelPopupClosed);
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(PopupType.LevelUpPopup);
            SendLevelReachedEvent(newLevel.Level);
        }

        private void OnLevelPopupClosed(object result)
        {
            _transitionRequested = (bool)result;
            SetState(TaskRewardFlowState.Idle);
        }

        private async void OnFlowCompleted()
        {
            _xpInfo = null;
            
            await _dataHolder.RefreshUserInfoAsync();

            FlowCompleted?.Invoke(_transitionRequested);
            _transitionRequested = false;
        }

        private Queue<SeasonLevel> GetUnlockedLevels()
        {
            var seasonLevels = _dataFetcher.CurrentSeason.Levels;
            var currentLevel = _xpInfo.CurrentLevel.Level;

            // Not enough for first level up -> no level up
            if (currentLevel == 1) return new Queue<SeasonLevel>();

            var xpBeforeTask = _xpInfo.Xp - _xpReward;

            // XP before task wasn't in range of previous level -> NO LEVEL UP
            if (_xpInfo.CurrentLevel.Xp < xpBeforeTask) return new Queue<SeasonLevel>();
            
            
            // find first
            var levelBeforeTask = GetLevelBeforeTask(xpBeforeTask);
            var queue = new Queue<SeasonLevel>();

            queue.Enqueue(levelBeforeTask);

            // Working under assumptions that season levels array will stay sorted in the ascending order
            foreach (var level in seasonLevels)
            {
                // if we were already on this level
                if (level.Level <= levelBeforeTask.Level) continue;
                // stop if we reached levels above our level after task 
                if (level.Level > currentLevel) break;
                
                queue.Enqueue(level);
            }
            
            return queue;
        }

        private bool UserLeveledUp()
        {
            return _unlockedLevels.Count != 0;
        }

        private SeasonLevel GetLevelBeforeTask(int xpBeforeTask)
        {
            var currentSeason = _dataFetcher.CurrentSeason;
            
            return currentSeason.Levels.Last(l => l.XpRequired <= xpBeforeTask);
        }

        private void SendLevelReachedEvent(int newLevel)
        {
            var levelUpMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.USER_LEVEL_REACHED] = newLevel,
                [AmplitudeEventConstants.EventProperties.SEASON_ID] = _dataFetcher.CurrentSeason.Id
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.USER_LEVEL_UP, levelUpMetaData);
        }

        private UserBalanceArgs GetAnimatedBalanceArgs(bool backwards)
        {
            var softCurrency = _dataHolder.UserBalance.SoftCurrencyAmount;
            var hardCurrency = _dataHolder.UserBalance.HardCurrencyAmount;
            
            var softFrom = backwards ? softCurrency - _softCurrencyReward : softCurrency;
            var softTo = backwards ? softCurrency : softCurrency + _softCurrencyReward;
            var hardFrom = backwards ? hardCurrency - _hardCurrencyReward : hardCurrency;
            var hardTo = backwards ? hardCurrency : hardCurrency + _hardCurrencyReward;
            
            return new UserBalanceArgs(0.0f, 0.5f, softFrom, softTo, hardFrom, hardTo);
        }
        
        private UserBalanceArgs GetStaticBalanceArgs()
        {
            var softCurrency = _dataHolder.UserBalance.SoftCurrencyAmount;
            var hardCurrency = _dataHolder.UserBalance.HardCurrencyAmount;
            
            var soft = _backwards ? softCurrency - _softCurrencyReward : softCurrency;
            var hard = _backwards ? hardCurrency - _hardCurrencyReward : hardCurrency;
            
            return new UserBalanceArgs(0.0f, 0.0f, soft, soft, hard, hard);
        }

        [Button("Test Flow")]
        private void TestFlow()
        {
            StartAnimation(100, 100, 0);
        }
        
        [Button("Test Multi Flow")]
        private void TestMultiFlow()
        {
            StartAnimation(200, 100, 0);
        }

        [Button]
        private void TestQuestFlow(int xpAmount)
        {
            StartAnimation(xpAmount, 0, 0);
        }
    }
}*/