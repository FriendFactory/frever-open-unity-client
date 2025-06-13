using Navigation.Core;
using static Navigation.Args.DiscoverySearchState;

namespace Navigation.Args
{
    public sealed class DiscoveryPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.DiscoveryPage;

        public int TabIndex { get; set; }
        public DiscoverySearchState SearchState { get; set; }
        public string SearchText { get; set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public DiscoveryPageArgs(int tabIndex = 0, DiscoverySearchState searchState = Disabled, string searchText = "")
        {
            TabIndex = tabIndex;
            SearchState = searchState;
            SearchText = searchText;
        }
    }
}