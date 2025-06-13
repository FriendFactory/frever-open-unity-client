using System;
using Bridge.Models.VideoServer;
using Navigation.Args;

namespace UIManaging.Common.Args.Views.LevelPreviews
{
    public class LevelPreviewItemArgs : BaseLevelItemArgs
    {
        public LevelPreviewItemArgs(Video video, Action<BaseLevelItemArgs> onClick, bool showScore = false, bool showLikes = true)
            : base(video: video, onClick: onClick, showScore: showScore, showLikes: showLikes)
        { }
    }
}