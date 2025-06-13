using TMPro;
using UIManaging.Localization;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews
{
    internal sealed class ChatIntroHandler : MonoBehaviour
    {
        private const string INTRO_KEY = "Frever.DirectMessages.IntroMessage";
       
        [SerializeField] private GameObject _motdPanel;
        [SerializeField] private TMP_Text _motdText;
        [SerializeField] private Button _closeButton;

        [Inject] private ChatLocalization _localization;
        
        private long _chatId;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize(IChatModel chatModel)
        {
            _chatId = chatModel.ChatId;
            CheckIfAlreadyShown();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CheckIfAlreadyShown()
        {
            var key = $"{INTRO_KEY}.{_chatId}";
            if (PlayerPrefs.HasKey(key)) Hide(); else Show();
        }

        private void Show()
        {
            _motdText.text = _localization.IntroPanelTitle;
            _motdPanel.SetActive(true);
        }

        private void Hide()
        {
            _motdPanel.SetActive(false);
        }

        private void OnCloseButtonClicked()
        {
            UpdatePrefs(_chatId);
            Hide();
        }

        private static void UpdatePrefs(long chatId)
        {
            var key = $"{INTRO_KEY}.{chatId}";
            PlayerPrefs.SetInt(key, 1);
            PlayerPrefs.Save();
        }
    }
}