using UIManaging.Pages.Common.VideoDetails.VideoAttributes;
using UnityEngine;

namespace UIManaging.Pages.Feed.Core
{
    /// <summary>
    /// Template video attribute example for backward compatibility with FeedVideoView.
    /// /// </summary>
    public class FeedTemplateVideoAttribute: TemplateVideoAttribute
    {
        [SerializeField] private TemplateUsedForVideoButton _usedTemplateButton;

        protected override void OnTemplateButtonShown()
        {
            base.OnTemplateButtonShown();
            
            var template = ContextData.Video.MainTemplate;

            _usedTemplateButton.Initialize(template.Id, template.Title, ContextData.OnJoinTemplateClick);
        }
    }
}