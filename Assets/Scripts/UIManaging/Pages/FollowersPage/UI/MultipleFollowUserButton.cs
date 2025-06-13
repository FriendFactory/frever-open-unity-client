using System.Threading.Tasks;
using UIManaging.Common.Args.Buttons;
using UIManaging.Pages.Common.FollowersManagement;
using Zenject;

namespace UIManaging.Pages.FollowersPage.UI
{
    public class MultipleFollowUserButton : BaseFollowUserButton<MultipleFollowUserButtonArgs>
    {
        protected override void OnFollowButtonClicked()
        {
            ContextData.OnFollow?.Invoke(ContextData.UserGroupId);
            RefreshUI(true);
        }
        
        protected override void OnUnfollowButtonClickedInternal()
        {
            ContextData.OnUnfollow?.Invoke(ContextData.UserGroupId);
            RefreshUI(false);
        }
    }
}


