using Navigation.Args;

namespace UIManaging.Common
{
    public class NavBarCreateButton : NavBarButtonBase
    {
        protected override void OnButtonClicked() => PageManager.MoveNext(new CreatePostPageArgs());
    }
}