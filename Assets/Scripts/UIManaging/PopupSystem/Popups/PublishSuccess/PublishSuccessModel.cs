using Bridge.Models.VideoServer;
using UIManaging.PopupSystem.Popups.PublishSuccess.VideoSharing;

namespace UIManaging.PopupSystem.Popups.PublishSuccess
{
    public sealed class PublishSuccessModel
    {
        public Video Video { get; }
        public string GenerateTemplateWithName { get; }
        public CreatorScoreModel CreatorScoreModel { get; }
        public VideoSharingModel VideoSharingModel { get; }

        public PublishSuccessModel(Video video, CreatorScoreModel creatorScoreModel, VideoSharingModel videoSharingModel, string generateTemplateWithName = null)
        {
            Video = video;
            CreatorScoreModel = creatorScoreModel;
            VideoSharingModel = videoSharingModel;
            GenerateTemplateWithName = generateTemplateWithName;
        }
    }
}