using System;
using Extensions;
using UIManaging.Common.SearchPanel;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Common.SongOption.Search
{
    public class MusicSearchPanelView: SearchPanelView
    {
        [SerializeField] private Button _requestSearchButton;

        public event Action<string> SearchRequested;

        protected override void Awake()
        {
            _requestSearchButton.SetActive(false);
            _requestSearchButton.onClick.AddListener(OnSearchClicked);

            ControlsVisibilityChanged += OnControlsVisibilityChanged;
            
            base.Awake();
        }

        public void ToggleControls(bool isOn)
        {
            _requestSearchButton.SetActive(isOn);
            _clearInputButton.SetActive(isOn);
        }

        private void OnControlsVisibilityChanged(bool state)
        {
            _requestSearchButton.SetActive(state);
        }

        private void OnDestroy()
        {
            _requestSearchButton.onClick.RemoveListener(OnSearchClicked);
            
            ControlsVisibilityChanged -= OnControlsVisibilityChanged;
        }

        private void OnSearchClicked()
        {
            SearchRequested?.Invoke(Text);
        }
    }
}