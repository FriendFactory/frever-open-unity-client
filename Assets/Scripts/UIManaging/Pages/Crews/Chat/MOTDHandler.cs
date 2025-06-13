using Bridge.Models.ClientServer.Crews;
using Modules.Crew;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews
{
    internal sealed class MOTDHandler : MonoBehaviour
    {
        private const string MOTD_KEY = "Frever.Crews.MOTD";

        [SerializeField] private GameObject _motdPanel;
        [SerializeField] private TMP_Text _motdText;
        [SerializeField] private Button _closeButton;

        [Inject] private CrewService _crewService;

        private string _lastMessage;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClicked);
            _crewService.MotDUpdated += OnMOTDUpdated;
        }

        private void OnDestroy()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClicked);
            _crewService.MotDUpdated -= OnMOTDUpdated;
            CleanUp();
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void Initialize(CrewModel crewModel)
        {
            _lastMessage = crewModel.MessageOfDay;
            CheckForUpdate();
        }

        public void CleanUp()
        {
            _lastMessage = string.Empty;
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void CheckForUpdate()
        {
            if (PlayerPrefs.HasKey(MOTD_KEY) && PlayerPrefs.GetString(MOTD_KEY) == _lastMessage) return;

            if (string.IsNullOrEmpty(_lastMessage))
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void Show()
        {
            _motdText.text = _lastMessage;
            _motdPanel.SetActive(true);
        }

        private void Hide()
        {
            _motdPanel.SetActive(false);
        }

        private void OnMOTDUpdated(string message)
        {
            _lastMessage = message;
            CheckForUpdate();
        }

        private void OnCloseButtonClicked()
        {
            UpdatePrefs(_lastMessage);
            Hide();
        }

        private static void UpdatePrefs(string message)
        {
            PlayerPrefs.SetString(MOTD_KEY, message);
            PlayerPrefs.Save();
        }
    }
}