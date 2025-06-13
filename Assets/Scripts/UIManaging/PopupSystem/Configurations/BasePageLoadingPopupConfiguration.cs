using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UIManaging.PopupSystem.Configurations
{
    public abstract class BasePageLoadingPopupConfiguration : PopupConfiguration
    {
        public readonly string Header;
        public readonly string ProgressBarText;

        public Action FadeInCompleted;
        public Action HideActionRequested;
        public Action CancelActionRequested;

        private List<string> _sceneNames;

        protected BasePageLoadingPopupConfiguration(string header, string progressBarText, Action<object> onClose, PopupType popupType) 
            : base(popupType, onClose)
        {
            Header = header;
            ProgressBarText = progressBarText;
        }

        public void Hide()
        {
            HideActionRequested?.Invoke();
        }

        public void HideAfterSceneSwitch(List<string> sceneNames = null)
        {
            _sceneNames = sceneNames;
            SceneManager.sceneLoaded += FadeOut;
        }

        public void CleanUp()
        {
            SceneManager.sceneLoaded -= FadeOut;
        }

        private async void FadeOut(Scene arg0, LoadSceneMode loadSceneMode)
        {
            if (_sceneNames == null || !_sceneNames.Contains(arg0.name)) return;

            SceneManager.sceneLoaded -= FadeOut;
            // Delay to avoid low frame rate caused by scene change
            await Task.Delay(TimeSpan.FromSeconds(0.5f));
            HideActionRequested?.Invoke();
        }
    }
}