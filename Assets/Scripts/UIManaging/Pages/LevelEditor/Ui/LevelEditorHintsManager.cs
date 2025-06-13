using System;
using JetBrains.Annotations;
using Navigation.Core;
using TipsManagment;
using TipsManagment.Args;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui
{
    [UsedImplicitly]
    internal sealed class LevelEditorHintsManager: IInitializable, IDisposable
    {
        private readonly TipManager _tipsManager;
        private readonly PopupManager _popupManager;
        private readonly LevelEditorPageModel _levelEditorPageModel;

        public LevelEditorHintsManager(TipManager tipsManager, PopupManager popupManager, LevelEditorPageModel levelEditorPageModel)
        {
            _tipsManager = tipsManager;
            _popupManager = popupManager;
            _levelEditorPageModel = levelEditorPageModel;
        }

        public void Initialize()
        {
            _popupManager.PopupHidden += OnPopupHidden;
        }

        private void OnPopupHidden(PopupType popupType)
        {
            if (popupType != PopupType.CameraPermission) return;
            
            _popupManager.PopupHidden -= OnPopupHidden;

            if (_levelEditorPageModel.EditorState == LevelEditorState.Default)
            {
                ShowHints();
                return;
            }
            
            _levelEditorPageModel.StateChanged += OnEditorStateChanged;
        }

        private void OnEditorStateChanged(LevelEditorState state)
        {
            if (state != LevelEditorState.Default) return;
            
            _levelEditorPageModel.StateChanged -= OnEditorStateChanged;
            
            ShowHints();
        }

        public void Dispose()
        {
            _popupManager.PopupHidden -= OnPopupHidden;
            _levelEditorPageModel.StateChanged -= OnEditorStateChanged;
        }

        private void ShowHints()
        {
            _tipsManager.ShowTipsByPageId(PageId.LevelEditor, TipType.PageParallel);
        }
    }
}