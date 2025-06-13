using Bridge.Models.VideoServer;

namespace UIManaging.Pages.Common.VideoDetails.VideoAttributes
{
    public class VideoDetailsModel
    {
        public Video Video { get; }
        public string GenerateTemplateWithName { get; }
        
        public VideoDetailsModel(Video video, string generateTemplateWithName = null)
        {
            Video = video;
            GenerateTemplateWithName = generateTemplateWithName;
        }

    }
}