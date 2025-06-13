using Navigation.Args;

namespace UIManaging.Common
{
    public sealed class NavBarProfileButton : NavBarButtonBase
    {
        protected override void OnButtonClicked() => PageManager.MoveNext(new UserProfileArgs());
    }
}