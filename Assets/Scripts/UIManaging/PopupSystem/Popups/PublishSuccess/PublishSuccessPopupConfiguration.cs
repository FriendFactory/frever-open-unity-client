using Bridge.Models.VideoServer;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.PublishSuccess.VideoSharing;

namespace UIManaging.PopupSystem.Popups.PublishSuccess
{
    public class PublishSuccessPopupConfiguration: PopupConfiguration
    {
        public Video Video { get; }
        public string GenerateTemplateWithName { get; }
        public CreatorScoreModel CreatorScoreModel { get; }
        public VideoSharingModel VideoSharingModel { get; }

        public PublishSuccessPopupConfiguration(Video video, CreatorScoreModel creatorScoreModel, VideoSharingModel videoSharingModel, string generateTemplateWithName = null) : base(PopupType.PublishSuccess, null, string.Empty)
        {
            Video = video;
            CreatorScoreModel = creatorScoreModel;
            VideoSharingModel = videoSharingModel;
            GenerateTemplateWithName = generateTemplateWithName;
        }
    }
}