using System;
using Navigation.Args;
using UIManaging.Pages.LevelEditor.Ui.SwitchingEditors;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.VideoMessage
{
    internal sealed class SwitchEditorOptionsPanel: SwitchEditorOptionsPanelBase
    {
        [SerializeField] private Button _enterLevelEditorButton;

        public event Action EnterLevelEditorButtonClicked;
        public event Action<NonLeveVideoData> GalleryVideoForUploadingPicked;

        protected override Button[] Buttons => new[] { UploadButton, _enterLevelEditorButton };

        public override void Init()
        {
            base.Init();
            _enterLevelEditorButton.onClick.AddListener(OnEnterLevelEditorButtonClicked);
        }

        public void SetEnterLevelEditorButtonInteractability(bool interactable)
        {
            _enterLevelEditorButton.interactable = interactable;
        }

        protected override void OnVideoSelected(NonLeveVideoData nonLeveVideoData)
        {
            GalleryVideoForUploadingPicked?.Invoke(nonLeveVideoData);
        }

        private void OnEnterLevelEditorButtonClicked()
        {
            EnterLevelEditorButtonClicked?.Invoke();
        }
    }
}