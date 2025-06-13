using System;
using Models;
using Navigation.Args;
using UIManaging.Common;
using UnityEngine;

namespace UIManaging.Pages.PublishPage.Buttons
{
    internal sealed class LevelVideoMessageSettingsPanel: VideoMessageSettingsPanelBase
    {
        [SerializeField] private LevelThumbnail _levelThumbnail;
        [SerializeField] private PreviewLevelButton _previewButton;

        private bool _initialized;
        private Level _level;

        public void Init(Level level, Action onPreviewClicked, VideoUploadingSettings settings)
        {
            if (_initialized) return;
            _initialized = true;
            _level = level;
            _previewButton.SetCallback(onPreviewClicked);

            if (settings.MessagePublishInfo != null)
            {
                Message = settings.MessagePublishInfo.MessageText;
            }
            base.Init();
        }

        public override void Show()
        {
            base.Show();
            if (!_levelThumbnail.IsInitialized)
            {
                _levelThumbnail.Initialize(_level);
            }
        }
    }
}