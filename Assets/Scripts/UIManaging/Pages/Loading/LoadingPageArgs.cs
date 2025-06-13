using System;
using Navigation.Core;

namespace UIManaging.Pages.Loading
{
    public sealed class LoadingPageArgs: PageArgs
    {
        public Action OnDataFetchedAction;

        public bool WaitForStartPack = false;
        public bool FetchDefaults = false;
        public bool ForceRefetch = false;

        public override PageId TargetPage { get; }

        public LoadingPageArgs(PageId pageId, bool force = false)
        {
            TargetPage = pageId;
            ForceRefetch = force;
        }
    }
}