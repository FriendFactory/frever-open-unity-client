using Bridge.Models.VideoServer;
using UIManaging.Pages.Common.VideoDetails.VideoAttributes;

namespace UIManaging.PopupSystem.Popups.PublishSuccess
{
    public class PublishSuccessTemplateVideoAttribute: TemplateVideoAttribute
    {
        protected override bool ShouldBeVisible()
        {
            if (string.IsNullOrEmpty(ContextData.GenerateTemplateWithName))
            {
                return base.ShouldBeVisible();
            }

            ContextData.Video.MainTemplate = new VideoTemplateInfo() { Title = ContextData.GenerateTemplateWithName };
            
            OnTemplateButtonShown();

            return true;
        }
    }
}