using System.Linq;
using Abstract;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Share
{
    internal sealed class ConfirmButton : BaseContextDataButton<ShareSelectionPanelModel>
    {
        [SerializeField] private TMP_Text _text;
        
        public bool Interactable
        {
            get => _button.interactable;
            set => _button.interactable = value;
        }
        
        public string Text
        {
            get => _text.text;
            set => _text.text = value;
        }
        
        protected override void OnInitialized()
        {
            SetupInteractable();
            Text = ContextData.ConfirmButtonText;
            ContextData.ItemSelectionChanged += OnSelectionChanged;
        }

        protected override void BeforeCleanup()
        {
            if (ContextData != null)
            {
                ContextData.ItemSelectionChanged -= OnSelectionChanged;
            }
        }

        private void OnSelectionChanged(ShareSelectionItemModel _)
        {
            SetupInteractable();
        }

        private void SetupInteractable()
        {
            Interactable = !ContextData.BlockConfirmButtonIfNoSelection || ContextData.SelectedItems.Count > 0;
        }

        protected override void OnUIInteracted()
        {
            base.OnUIInteracted();
            ContextData.OnConfirmed?.Invoke(GetSelectedData());
        }

        private ShareDestinationData GetSelectedData()
        {
            return new ShareDestinationData
            {
                Chats = ContextData.SelectedItems.Where(x => x is ShareSelectionChatsItemModel)
                                   .Cast<ShareSelectionChatsItemModel>().Select(x => x.ChatShortInfo)
                                   .ToArray(),
                Users = ContextData.SelectedItems.Where(x => x is ShareSelectionFriendsItemModel)
                                    .Cast<ShareSelectionFriendsItemModel>().Select(x => x.Profile)
                                    .ToArray()
            };
        }
    }
}