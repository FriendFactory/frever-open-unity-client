using Navigation.Args;
using UIManaging.EnhancedScrollerComponents;

namespace UIManaging.Pages.DiscoveryPage
{
    internal sealed class DiscoveryVideoRow : EnhancedScrollerItemsRow<DiscoveryVideoItem, BaseLevelItemArgs>
    {
        public void Refresh()
        {
            if (Views == null) return;
            foreach (var view in Views)
            {
                view.Refresh();
            }
        }
    }
}