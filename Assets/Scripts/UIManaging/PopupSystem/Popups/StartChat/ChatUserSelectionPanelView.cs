using UIManaging.Pages.Common.Helpers;
using UIManaging.Pages.UserSelection;
using Zenject;

namespace UIManaging.PopupSystem.Popups.StartChat
{
    public class ChatUserSelectionPanelView: UserSelectionPanelView
    {
        [Inject] private CharacterThumbnailsDownloader _thumbnailsDownloader;
        
        protected override void BeforeCleanup()
        {
            if (ContextData != null)
            {
                _thumbnailsDownloader.ClearCachedCharacterThumbnails();
            }
            
            base.BeforeCleanup();
        }
    }
}