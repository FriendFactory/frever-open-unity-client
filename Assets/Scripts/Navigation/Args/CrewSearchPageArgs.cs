using System;
using Navigation.Core;

namespace Navigation.Args
{
    public class CrewSearchPageArgs : PageArgs
    {
        public override PageId TargetPage => PageId.CrewSearch;

        public string SearchQuery = String.Empty;
    }
}