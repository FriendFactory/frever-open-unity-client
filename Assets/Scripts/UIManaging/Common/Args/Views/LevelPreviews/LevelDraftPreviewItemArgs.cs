using Models;
using Navigation.Args;
using System;

namespace UIManaging.Common.Args.Views.LevelPreviews
{
    public sealed class LevelDraftPreviewItemArgs : BaseLevelItemArgs
    {
        public LevelDraftPreviewItemArgs(Level level, Action<BaseLevelItemArgs> onClick) 
            : base(level, showEditButton: true, showCreationDate: true, onClick: onClick)
        { }
    }
}