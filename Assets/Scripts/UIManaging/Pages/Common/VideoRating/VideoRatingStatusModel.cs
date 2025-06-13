using JetBrains.Annotations;

namespace UIManaging.Pages.Common.VideoRating
{
    [UsedImplicitly]
    public sealed class VideoRatingStatusModel
    {
        public long LevelId { get; set; }
        public bool IsCompleted { get; set; }
    }
}