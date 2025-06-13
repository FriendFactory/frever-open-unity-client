using System;
using System.Linq;
using Bridge;
using Bridge.Models.ClientServer.Battles;
using Common.UserBalance;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks.RewardPopUp;
using UIManaging.PopupSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.VotingResult
{
    internal sealed class VotingResultPage : GenericPage<VotingResultPageArgs>
    {
        private const int PLAY_PARTICLES_PLACE_MAX = 3;
        
        public override PageId Id => PageId.VotingResult;

        [SerializeField] private TMP_Text _taskName;
        [SerializeField] private VotingVideoListControl _videoListControl;
        [SerializeField] private VotingResultListView _votingResultListView;
        [SerializeField] private VotingResultsViewAnimator _votingResultsViewAnimator;
        [SerializeField] private FlyingRewardsAnimationController _flyingRewardsAnimationController;
        [SerializeField] private UserBalanceView _userBalanceView;
        
        [Inject] private IBridge _bridge;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private LocalUserDataHolder _localUserData;
        [Inject] private IVotingBattleResultManager _votingBattleResultManager;
        
        private BattleResult[] _battleResults;
        
        protected override void OnInit(PageManager pageManager)
        {
        }

        protected override async void OnDisplayStart(VotingResultPageArgs args)
        {
            SetupHeader(args);
            _battleResults = OpenPageArgs.BattleResults ??
                             await _votingBattleResultManager.GetVotingBattleResult(args.TaskId);
            
            SetupResultsView(args.TaskId, _battleResults);
            
            var place = GetLocalUserPlace(_battleResults);
            
            _userBalanceView.Initialize(new StaticUserBalanceModel(_localUserData));
            
            var userAlreadyClaimedReward = await _votingBattleResultManager.IsClaimed(args.TaskId);
            
            if (userAlreadyClaimedReward)
            {
                FocusOnLocalUserResult(_battleResults);
            }
            else
            {
                var localUserRewards = _battleResults.FirstOrDefault(x => x.Group != null && x.Group.Id == _bridge.Profile.GroupId)?.SoftCurrency ?? 0;
                
                _popupManagerHelper.ShowVotingResultCongratsPopup(place, place <= PLAY_PARTICLES_PLACE_MAX, localUserRewards, OnPopupClosed);
            }
            
            base.OnDisplayStart(args);
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _votingResultListView.Cleanup();
            base.OnHidingBegin(onComplete);
        }

        private void SetupHeader(VotingResultPageArgs args)
        {
            _taskName.text = args.TaskName;
        }

        private void SetupResultsView(long taskId, BattleResult[] battleResults)
        {
            var video = battleResults.OrderByDescending(x => x.Score)
                                     .Where(x=>x.Video!=null && x.Group != null)
                                     .Select(x=> x.Video);
            _videoListControl.Initialize(taskId, video.ToArray());
            _votingResultListView.Initialize(new VotingResultListModel(battleResults.Where(x => x.Group != null)));
        }

        private void FocusOnLocalUserResult(BattleResult[] battleResults)
        {
            var localUserResult = GetLocalUserBattleResult(battleResults);
            var localUserView = _votingResultListView.GetView(localUserResult);
            _votingResultsViewAnimator.ScrollTo(localUserView.GetComponent<RectTransform>());
        }

        private BattleResult GetLocalUserBattleResult(BattleResult[] battleResults)
        {
            return battleResults.First(x => x.Group != null && x.Group.Id == _bridge.Profile.GroupId);
        }

        private int GetLocalUserPlace(BattleResult[] battleResults)
        {
            return battleResults.OrderByDescending(x => x.Score)
                                .TakeWhile(x => x.Group == null || x.Group.Id != _bridge.Profile.GroupId).Count() + 1;
        }
        
        private async void OnPopupClosed()
        {
            FocusOnLocalUserResult(_battleResults);
            var battleResult = GetLocalUserBattleResult(_battleResults);
            var claimResult = await _bridge.ClaimVotingBattleReward(OpenPageArgs.TaskId);
             if (claimResult.IsSuccess)
             {
                 PlayRewardsClaimedAnimations(battleResult);
                 await _localUserData.UpdateBalance();
             }
             else
             {
                 Debug.LogError("Failed to claim reward");
             }
        }

        private void PlayRewardsClaimedAnimations(BattleResult battleResult)
        {
            _flyingRewardsAnimationController.Play(true, false, false);
            _flyingRewardsAnimationController.LastElementReachedTarget += ShowRatePopup;
            var balance = _localUserData.UserBalance;
            var animationArgs = new UserBalanceArgs(1.5f, 1f, balance.SoftCurrencyAmount, balance.SoftCurrencyAmount + battleResult.SoftCurrency, balance.HardCurrencyAmount, balance.HardCurrencyAmount);
            var balanceModel = new AnimatedUserBalanceModel(animationArgs);
            _userBalanceView.Initialize(balanceModel);
        }

        private void ShowRatePopup()
        {
            _flyingRewardsAnimationController.LastElementReachedTarget -= ShowRatePopup;
            
            const string key = "LastTimeRateAppAskedOnStyleChallenge";
            var lastTimeAskedWeek = PlayerPrefs.GetInt(key, -1);
            var currentWeek = System.Globalization.ISOWeek.GetWeekOfYear(DateTime.UtcNow);

            if (GetLocalUserPlace(_battleResults) != 1
                || lastTimeAskedWeek == currentWeek)
            {
                return;
            }
            
            PlayerPrefs.SetInt(key, currentWeek);
            _popupManagerHelper.ShowRateAppPopup();
        }
    }
}
