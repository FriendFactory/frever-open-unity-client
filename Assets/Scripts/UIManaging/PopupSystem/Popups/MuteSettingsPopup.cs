using Bridge.Scripts.ClientServer.Chat;
using Common.Permissions;
using Extensions;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups
{
    public class MuteSettingsPopup : ConfigurableBasePopup<MuteSettingsPopupConfiguration>
    {
        [SerializeField] private TMP_Text _title;
        
        [SerializeField] private Button _mute15MinButton;
        [SerializeField] private Button _mute1HourButton;
        [SerializeField] private Button _mute8HoursButton;
        [SerializeField] private Button _mute24HoursButton;
        [SerializeField] private Button _muteForeverButton;
        
        private void Awake()
        {
            _mute15MinButton.onClick.AddListener(() => Hide((int)MuteChatTimeOptions.For15Minutes));
            _mute1HourButton.onClick.AddListener(() => Hide((int)MuteChatTimeOptions.For1Hour));
            _mute8HoursButton.onClick.AddListener(() => Hide((int)MuteChatTimeOptions.For8Hours));
            _mute24HoursButton.onClick.AddListener(() => Hide((int)MuteChatTimeOptions.For24Hours));
            _muteForeverButton.onClick.AddListener(() => Hide((int)MuteChatTimeOptions.Permanent));
        }

        protected override void OnConfigure(MuteSettingsPopupConfiguration configuration)
        {
            _title.text = configuration.Title;
        }
    }

    public class MuteSettingsPopupConfiguration : PopupConfiguration { }
}