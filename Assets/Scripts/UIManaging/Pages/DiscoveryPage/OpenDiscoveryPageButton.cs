using Navigation.Args;
using Navigation.Core;
using UIManaging.Core;

namespace UIManaging.Pages.DiscoveryPage
{
    public class OpenDiscoveryPageButton : ButtonBase
    {
        protected override void OnClick()
        {
            Manager.MoveNext(PageId.DiscoveryPage, new DiscoveryPageArgs());
        }
    }
}