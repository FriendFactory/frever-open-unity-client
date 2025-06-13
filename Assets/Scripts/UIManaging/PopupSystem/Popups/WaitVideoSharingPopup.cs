using UIManaging.PopupSystem.Configurations;

namespace UIManaging.PopupSystem.Popups
{
    internal sealed class WaitVideoSharingPopup: BasePopup<WaitVideoSharingPopupConfiguration>
    {
        protected override void OnConfigure(WaitVideoSharingPopupConfiguration configuration)
        {
        }
    }

    public sealed class WaitVideoSharingPopupConfiguration: PopupConfiguration
    {
        public WaitVideoSharingPopupConfiguration(): base(PopupType.WaitVideoSharingPopup, null)
        {
        }
    }
}