using Extensions;
using Models;
using Navigation.Args;

namespace UIManaging.Pages.PublishPage
{
    public class TemplatePublishSettings
    {
        public bool IsVideoEligibleForTemplate { get; }
        public bool ShowTemplatePopup { get; }

        public TemplatePublishSettings(Level level, VideoUploadingSettings settings)
        {
            IsVideoEligibleForTemplate = !level.IsRemix()
                                      && !level.UsesTemplateEvent()
                                      && !level.IsVideoMessageBased();

            ShowTemplatePopup = IsVideoEligibleForTemplate && settings.PublishInfo.ShowTemplatePopup &&
                                level.GetFirstEvent().HasMusic();
        }
    }
}