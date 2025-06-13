using System;
using Models;
using Navigation.Args;

namespace UIManaging.Common.Args.Views.LevelPreviews
{
    public sealed class LevelDraftParentPreviewItemArgs : BaseLevelItemArgs
    {
        public LevelDraftParentPreviewItemArgs(Level level, bool isDraft, Action<BaseLevelItemArgs> onClick)
            : base(level, isDraft: isDraft, onClick: onClick)
        { }
    }
}