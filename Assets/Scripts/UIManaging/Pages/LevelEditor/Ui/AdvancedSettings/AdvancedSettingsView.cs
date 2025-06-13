using System;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.AdvancedSettings
{
    public abstract class AdvancedSettingsView : MonoBehaviour
    {
        public event Action Hidden;
        public event Action Shown;
        public event Action DiscardChangesButtonClicked;
        public event Action ConfirmChangesButtonClicked;
        public event Action SettingChanged;

        [SerializeField] public Button Done;
        [SerializeField] public Button Cancel;

        private ILevelManager _levelManager;
        protected bool IsSetup;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        public void Construct(ILevelManager levelManager)
        {
            _levelManager = levelManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public virtual void Setup()
        {
            _levelManager.EventLoadingCompleted += OnEventLoaded;
            IsSetup = true;
            OnEventLoaded();
        }

        public virtual void Display()
        {
            Shown?.Invoke();
            gameObject.SetActive(true);
            Done.onClick.AddListener(OnConfirm);
            Cancel.onClick.AddListener(OnDiscard);
        }

        public virtual void Hide()
        {
            if (!IsActive()) return;
            
            gameObject.SetActive(false);
            Done.onClick.RemoveAllListeners();
            Cancel.onClick.RemoveAllListeners();
            Hidden?.Invoke();
        }

        public virtual void CleanUp()
        {
            _levelManager.EventLoadingCompleted -= OnEventLoaded;
            IsSetup = false;
        }

        public abstract void ResetTabs();

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected void OnSettingChanged()
        {
            SettingChanged?.Invoke();
        }

        protected abstract void OnEventLoaded();
        
        protected virtual void OnDiscard()
        {
            Hide();
            DiscardChangesButtonClicked?.Invoke();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnConfirm()
        {
            Hide();
            ConfirmChangesButtonClicked?.Invoke();
        }

        private bool IsActive()
        {
            return gameObject.activeSelf && gameObject.activeInHierarchy;
        }
        
    }
}