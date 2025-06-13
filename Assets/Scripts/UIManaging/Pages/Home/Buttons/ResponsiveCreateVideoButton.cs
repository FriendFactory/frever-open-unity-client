using Navigation.Args;

namespace UIManaging.Pages.Home
{
    public sealed class ResponsiveCreateVideoButton : ResponsiveHomePageButtonBase
    {
        protected override void OnEnable()
        {
            base.OnEnable();
            Interactable = true;
        }

        protected override void OnClick()
        {
            Manager.MoveNext(new CreatePostPageArgs());
            Interactable = false;
        }
    }
}