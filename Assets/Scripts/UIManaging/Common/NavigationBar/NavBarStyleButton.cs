using Navigation.Args;

namespace UIManaging.Common
{
    public sealed class NavBarStyleButton : NavBarButtonBase
    {
        protected override void OnButtonClicked() => PageManager.MoveNext(new HomePageArgs());
    }
}