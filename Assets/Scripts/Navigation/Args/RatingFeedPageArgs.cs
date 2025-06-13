using System;
using Models;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class RatingFeedPageArgs : PageArgs
    {
        public Level LevelData;

        //---------------------------------------------------------------------
        // Actions
        //---------------------------------------------------------------------

        public Action MoveNextRequested;
        public Action SkipRatingRequested;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId TargetPage => PageId.RatingFeed;
    }
}