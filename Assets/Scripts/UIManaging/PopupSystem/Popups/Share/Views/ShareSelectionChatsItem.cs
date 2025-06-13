using Extensions;
using TMPro;
using UIManaging.Pages.Common.UsersManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Share
{
    public abstract class ShareSelectionChatsItem: ShareSelectionItem<ShareSelectionChatsItemModel>
    {
        [SerializeField] private TMP_Text _lastMessage;

        [Inject] protected LocalUserDataHolder DataHolder;
        
        protected override void OnInitialized()
        {
            base.OnInitialized();

            var message = ContextData.ChatShortInfo.LastMessageText;
            
            _lastMessage.text = message;
            
            _lastMessage.SetActive(!string.IsNullOrEmpty(message));
        }

        protected override void RefreshPortraitImage()
        {
            RefreshChatsPortraitImage();
        }

        protected abstract void RefreshChatsPortraitImage();
    }
}