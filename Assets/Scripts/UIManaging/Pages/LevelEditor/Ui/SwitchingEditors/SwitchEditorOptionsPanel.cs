using Extensions;
using Modules.LevelManaging.Editing.LevelManagement;
using Navigation.Args;
using UIManaging.PopupSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SwitchingEditors
{
    internal sealed class SwitchEditorOptionsPanel : SwitchEditorOptionsPanelBase
    {
        [SerializeField] private Button _videoMessageButton;

        [Inject] private LevelEditorPageModel _pageModel;
        [Inject] private PopupManagerHelper _popupManagerHelper;
        [Inject] private ILevelManager _levelManager;

        private bool _initialized;

        protected override Button[] Buttons => new[] { UploadButton, _videoMessageButton };

        public override void Init()
        {
            if (_initialized) return;
            _videoMessageButton.onClick.AddListener(OnVideoMessageClicked);
            base.Init();
            _initialized = true;
        }
        
        public override void Show()
        {
            base.Show();
            StartListeningToOutsideClickEvent();
        }

        public override void Hide()
        {
            base.Hide();
            StopListeningToOutsideClickEvent();
        }

        protected override void OnDestroy()
        {
            _videoMessageButton.onClick.RemoveListener(OnVideoMessageClicked);
        }

        protected override void OnVideoSelected(NonLeveVideoData nonLeveVideoData)
        {
            _pageModel.RequestNonLevelVideoUploading(nonLeveVideoData);
        }

        private void OnVideoMessageClicked()
        {
            _videoMessageButton.interactable = false;
            OutsideClickDetector.enabled = false;
            StopListeningToOutsideClickEvent();
            if (!_levelManager.CurrentLevel.IsEmpty() || _pageModel.HasUserChangedAnyAssetFromOriginalTemplate())
            {
                _popupManagerHelper.ShowStashEditorChangesPopup(RequestOpeningVideoMessageEditor, ()=>
                {
                    OutsideClickDetector.enabled = true;
                    _videoMessageButton.interactable = true;
                    StartListeningToOutsideClickEvent();
                });
            }
            else
            {
                RequestOpeningVideoMessageEditor();
            }
        }

        private void RequestOpeningVideoMessageEditor()
        {
            _pageModel.RequestOpeningVideoMessageEditor();
        }
    }
}
