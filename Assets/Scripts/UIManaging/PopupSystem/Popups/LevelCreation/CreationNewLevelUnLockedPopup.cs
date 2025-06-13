using System;
using Common;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.PopupSystem.Popups.LevelCreation
{
    public sealed class CreationNewLevelUnLockedPopupConfig : PopupConfiguration
    {
        public Action OnExploreLevelEditorButtonClicked;

        public CreationNewLevelUnLockedPopupConfig(Action onExploreLevelEditorButtonClicked): base(PopupType.CreationNewLevelUnLockedPopup, null)
        {
            OnExploreLevelEditorButtonClicked = onExploreLevelEditorButtonClicked;
        }
    }
    
    internal sealed class CreationNewLevelUnLockedPopup: BasePopup<CreationNewLevelUnLockedPopupConfig>
    {
        [SerializeField] private Button _exploreLevelEditorButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private ImageByKeyLoader _backgroundImageLoader;

        private void Awake()
        {
            _exploreLevelEditorButton.onClick.AddListener(() =>
            {
                Hide();
                Configs.OnExploreLevelEditorButtonClicked?.Invoke();
            });
            _closeButton.onClick.AddListener(Hide);
        }

        public override void Show()
        {
            base.Show();
            _backgroundImageLoader.LoadImageAsync(Constants.FileKeys.LEVEL_CREATION_UNLOCKED_BG);
        }

        protected override void OnConfigure(CreationNewLevelUnLockedPopupConfig configuration)
        {
        }
    }
}