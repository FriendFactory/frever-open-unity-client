using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups;

namespace UIManaging.Pages.Crews.Popups
{
    public class CrewCreationUnlockedPopup : CrewCreationLockedPopup
    {
        protected override void OnConfigure(AlertPopupConfiguration configuration)
        {
            InitCreatorScoreBadgeView();
        }
    }
}