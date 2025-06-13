using Navigation.Args;
using Navigation.Core;
using UIManaging.Core;

namespace UIManaging.Pages.Tasks
{
    public class CreatePostPageButton : ButtonBase
    {
        protected override void OnClick()
        {
            Manager.MoveNext(PageId.CreatePost, new CreatePostPageArgs());
        }
    }
}