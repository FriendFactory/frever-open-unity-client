using System;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal sealed class NonLevelVideoMessageSettingsPanel : VideoMessageSettingsPanelBase
    {
        [SerializeField] private Button _previewButton;
        [SerializeField] private RawImage _videoThumbnail;

        private Action _onPreviewClicked;

        private void Awake()
        {
            _previewButton.onClick.AddListener(OnPreviewClicked);
        }

        public void Init(Action onPreviewClicked, Texture2D videoThumbnail)
        {
            _onPreviewClicked = onPreviewClicked;
            _videoThumbnail.texture = videoThumbnail;
            base.Init();
        }

        private void OnPreviewClicked()
        {
            _onPreviewClicked?.Invoke();
        }
    }
}