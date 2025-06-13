using Laphed.Rx;

namespace UIManaging.Pages.PublishPage.VideoDetails.Attributes
{
    internal sealed class VideoPostUploadedAttribute: VideoPostAttribute<bool>
    {
        protected override ReactiveProperty<bool> Target => ContextData.UploadVideo;
        
        protected override void OnTargetValueChanged()
        {
            IsVisible.Value = Target.Value;
        }
    }
}