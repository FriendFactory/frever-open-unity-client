using System;
using Abstract;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.ChatSettings
{
    [RequireComponent(typeof(Button))]
    internal sealed class ChatMemberView : BaseContextDataView<ChatMemberModel>
    {
        [SerializeField] private Button _button;
        
        [Space]
        [SerializeField] private RawImage _portrait;
        [SerializeField] private TMP_Text _username;

        [Inject] private PopupManager _popupManager;
        
        private void Reset()
        {
            _button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveAllListeners();
        }

        protected override void OnInitialized()
        {
            _username.text = ContextData.UserInfo.Nickname;
            
            ContextData.DownloadThumbnail(OnDownloadSuccess);
        }

        private void OnButtonClick()
        {
            var config = new ChatUserInfoPopupConfiguration(ContextData.UserInfo, ContextData.HideAction);
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }
            
        private void OnDownloadSuccess(Texture2D texture)
        {
            _portrait.texture = texture;
        }
    }
}