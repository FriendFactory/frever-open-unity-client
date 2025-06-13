using System;
using Bridge.Models.VideoServer;
using Models;

namespace Navigation.Args
{
    public abstract class BaseLevelItemArgs : VideoThumbnailModel
    {
        public Level Level { get; }
        public Video Video { get; }
        public long Likes { get; }
        public bool ShowLikes { get; }
        public bool ShowDrafts { get; }
        public bool ShowCreationDate { get; }
        public bool ShowPreviewButton { get; }
        public bool ShowEditButton { get; }
        public bool ShowScore { get; }
        public bool IsInteractable => _onClick != null;

        private readonly Action<BaseLevelItemArgs> _onClick;

        protected BaseLevelItemArgs(Level level = null, Video video = null, bool isDraft = false,
            bool showCreationDate = false, bool showEditButton = false, bool showPreviewButton = false, bool showScore = false, bool showLikes = true,
            Action<BaseLevelItemArgs> onClick = null) : base(video?.ThumbnailUrl)
        {
            Level = level;
            Video = video;
            
            var likes = video?.KPI.Likes;
            Likes = likes ?? default;
            ShowLikes = showLikes && likes.HasValue;
            ShowDrafts = isDraft;
            
            ShowCreationDate = showCreationDate;
            ShowEditButton = showEditButton;
            ShowPreviewButton = showPreviewButton;
            ShowScore = showScore;
            _onClick = onClick;
        }

        public void OnInteracted()
        {
            _onClick?.Invoke(this);
        }
    }
}