using Bridge.Models.VideoServer;
using UnityEngine.Events;

namespace UIManaging.Pages.Common.VideoDetails.VideoAttributes
{
    public sealed class VideoAttributesModel
    {
        public Video Video { get; }
        public bool ShowBasedOnTemplateAttribute { get; }
        public UnityAction OnJoinTemplateClick { get; }
        public long OpenedWithTask { get; }
        public string GenerateTemplateWithName { get; }

        public VideoAttributesModel(Video video, UnityAction onJoinTemplateClick, long openedWithTask, bool showBasedOnTemplateAttribute = true, string generateTemplateWithName = null)
        {
            Video = video;
            OnJoinTemplateClick = onJoinTemplateClick;
            OpenedWithTask = openedWithTask;
            GenerateTemplateWithName = generateTemplateWithName;
            ShowBasedOnTemplateAttribute = showBasedOnTemplateAttribute;
        }
    }
}