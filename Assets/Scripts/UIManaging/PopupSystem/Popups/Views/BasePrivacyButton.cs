using System;
using TMPro;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.PopupSystem.Popups.Views
{
    public abstract class BasePrivacyButton<TAccess> : MonoBehaviour where TAccess : Enum
    {
        [Header("Optional")] [SerializeField] private Button targetButton;
        [SerializeField] private TextMeshProUGUI targetText;

        [Inject] private PopupManager _popupManager;

        private TAccess _access;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public TAccess Access
        {
            set
            {
                _access = value;

                UpdateText();

                OnAccessSet?.Invoke(value);
            }
            get => _access;
        }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action<TAccess> OnAccessSet;

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void UpdateText()
        {
            if (targetText != null)
            {
                targetText.text = ToText(_access);
            }
        }
        
        protected abstract string ToText(TAccess access);
        protected abstract BasePrivacyPopupConfiguration<TAccess> GetPopupConfiguration();

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            if (targetButton == null)
            {
                targetButton = GetComponent<Button>();
            }
        }

        private void OnEnable()
        {
            if (targetText != null)
            {
                targetText.text = ToText(_access);
            }
            
            targetButton.onClick.AddListener(OnClick);
        }

        private void OnDisable()
        {
            targetButton.onClick.RemoveListener(OnClick);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnClick()
        {
            var config = GetPopupConfiguration();

            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }
    }
}