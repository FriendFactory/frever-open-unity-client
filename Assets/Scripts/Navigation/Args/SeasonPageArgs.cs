using System;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class SeasonPageArgs : PageArgs
    {
        public Tab StartingTab { get; }
        public long? StartQuestId { get; }
        public Action MoveBack { get; }

        public override PageId TargetPage => PageId.SeasonInfo;

        public SeasonPageArgs(Tab startingTab, long? startQuestId = null, Action moveBack = null)
        {
            StartingTab = startingTab;
            StartQuestId = startQuestId;
            MoveBack = moveBack;
        }

        //---------------------------------------------------------------------
        // Nested
        //---------------------------------------------------------------------

        public enum Tab
        {
            Rewards,
            Quests
        }
    }
}