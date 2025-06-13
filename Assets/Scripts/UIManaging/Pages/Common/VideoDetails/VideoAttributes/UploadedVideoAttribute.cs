namespace UIManaging.Pages.Common.VideoDetails.VideoAttributes
{
    internal sealed class UploadedVideoAttribute: VideoAttribute
    {
        protected override bool ShouldBeVisible() => !Video.LevelId.HasValue;
    }
}