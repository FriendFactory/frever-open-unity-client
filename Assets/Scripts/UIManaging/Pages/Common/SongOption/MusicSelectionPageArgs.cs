using Navigation.Core;

namespace UIManaging.Pages.Common.SongOption
{
    public class MusicSelectionPageArgs: PageArgs
    {
        public override PageId TargetPage => PageId.MusicSelection;

        public SelectionPurpose SelectionPurpose;
    }
    
    public enum SelectionPurpose
    {
        ForRecordingNewEvent,
        ForReplacing
    }
}