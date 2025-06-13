using Navigation.Args;
using Navigation.Core;
using UIManaging.Core;

namespace UIManaging.Pages.Authorization.Ui
{
    public class ProfileButton : ButtonBase
    {
        protected override void OnClick()
        {
            if (Manager.IsChangingPage) return;

            var args = new UserProfileArgs();
            Manager.MoveNext(PageId.UserProfile, args);
        }
    }
}