using System.Threading.Tasks;
using Common.Abstract;
using Common.UserBalance;
using UIManaging.Animated;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.Tasks.RewardPopUp;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Tasks.TaskRewardFlow
{
    public sealed class FlyingCoinsAnimationController: BaseContextPanel<RewardModel>
    {
        [SerializeField] private UserBalanceView _userBalanceView;
        [SerializeField] private FlyingRewardsAnimationController _flyingRewardsAnimationController;
        [Header("Animation")]
        [SerializeField] private float _startDelay;
        [SerializeField] private float _duration = 1f;
        
        [Inject] private LocalUserDataHolder _localUserDataHolder;
        
        protected override void OnInitialized()
        {
            var userBalanceModel = new StaticUserBalanceModel(_localUserDataHolder);
            
            _userBalanceView.Initialize(userBalanceModel);
        }
        
        protected override void BeforeCleanUp()
        {
            _userBalanceView.CleanUp();
        }

        public async Task PlayAnimationAsync()
        {
            if (_userBalanceView.IsInitialized)
            {
                _userBalanceView.ContextData.CleanUp();
                _userBalanceView.CleanUp();
            }
            
            var tcs = new TaskCompletionSource<bool>();

            _flyingRewardsAnimationController.FirstElementReachedTarget += PlayUserBalanceAnimation;
            _flyingRewardsAnimationController.LastElementReachedTarget += OnAnimationFinished;
            
            _flyingRewardsAnimationController.Play(true, false, false);
            
            await tcs.Task;

            void PlayUserBalanceAnimation()
            {
                _flyingRewardsAnimationController.FirstElementReachedTarget -= PlayUserBalanceAnimation;
            
                var userBalance = _localUserDataHolder.UserBalance;
                var fromSoft = userBalance.SoftCurrencyAmount;
                var toSoft = userBalance.SoftCurrencyAmount + ContextData.SoftCurrencyReward;
                var hardAmount = _localUserDataHolder.UserBalance.HardCurrencyAmount;

                var args = new UserBalanceArgs(_startDelay, _duration, fromSoft, toSoft, hardAmount, hardAmount);

                _userBalanceView.Initialize(new AnimatedUserBalanceModel(args));

                _localUserDataHolder.UserBalance.SoftCurrencyAmount = toSoft;
            }

            void OnAnimationFinished()
            {
                _flyingRewardsAnimationController.LastElementReachedTarget -= OnAnimationFinished;

                tcs.SetResult(true);
            }
        }
    }
}