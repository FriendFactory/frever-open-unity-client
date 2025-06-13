using System;
using System.Threading;
using System.Threading.Tasks;
using UIManaging.Common.Args.Buttons;
using UIManaging.Pages.Common.FollowersManagement;
using Zenject;

namespace UIManaging.Pages.FollowersPage.UI
{
    public class FollowUserButton : BaseFollowUserButton<FollowUserButtonArgs>
    {
        private const int CANCEL_REQUEST_PERIOD_MILLI = 500;
        
        [Inject] private FollowersManager _followersManager;
        
        private long _followTargetGroupId;
        private CancellationTokenSource _tokenSource;

        protected override async void OnFollowButtonClicked()
        {
            RefreshUI(true);
            OnBeforeFollowChange();

            var wasCancelled = await CancelPeriod();
            if (wasCancelled || ContextData != null && ContextData.IsFollowing) return;

            _followersManager.FollowUser(_followTargetGroupId, ()=>OnFollowChangeSuccess(true), OnFollowChangeFailed);
        }

        protected override async void OnUnfollowButtonClickedInternal()
        {
            RefreshUI(false);
            OnBeforeFollowChange();

            var wasCancelled = await CancelPeriod();
            if (wasCancelled || ContextData != null && !ContextData.IsFollowing) return;
            
            _followersManager.UnfollowUser(_followTargetGroupId, ()=>OnFollowChangeSuccess(false), OnFollowChangeFailed);
        }

        private void OnBeforeFollowChange()
        {
            _tokenSource?.Cancel();
            _followTargetGroupId = ContextData.UserGroupId;
        }

        private void OnFollowChangeSuccess(bool followed)
        {
            if (ContextData == null || IsDestroyed)
            {
                return;
            }

            if (_followTargetGroupId != ContextData.UserGroupId) return;
            OnFollowStatusUpdated(followed);
        }
        
        private void OnFollowChangeFailed()
        {
            if (ContextData == null || IsDestroyed || _followTargetGroupId != ContextData.UserGroupId)
            {
                return;
            }
            
            OnFollowStatusUpdated(ContextData.IsFollowing);
            RefreshUI(ContextData.IsFollowing);
        }

        private async Task<bool> CancelPeriod()
        {
            try
            {
                _tokenSource = new CancellationTokenSource();
                await Task.Delay(CANCEL_REQUEST_PERIOD_MILLI, _tokenSource.Token);
                return false;
            }
            catch (OperationCanceledException)
            {
                return true;
            }
        }
    }
}


