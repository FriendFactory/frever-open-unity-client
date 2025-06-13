using Extensions;
using TMPro;
using UIManaging.Pages.Common.SongOption;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.ProgressTracking;
using UIManaging.Pages.LevelEditor.Ui.EditingFlow.TemplateSetup;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.EditingFlow
{
    internal sealed class TemplateSetupStepView : BaseEditingStepView
    {
        [SerializeField] private TMP_Text _headerText;
        
        [Inject] private TemplateSetupStepProgress _progress;
        [Inject] private MusicSelectionPageModel _musicSelectionPageModel;
        [Inject] private TemplateSetupProgressTracker _progressTracker;
        [Inject] private LevelEditorPageModel _levelEditorPageModel;
        
        private bool IsFirstOpen { get; set; } = true;
        
        public override LevelEditorState State => LevelEditorState.TemplateSetup;

        protected override void OnInitialized()
        {
            base.OnInitialized();

            _progress.Initialize();
        }

        protected override void OnShow()
        {
            base.OnShow();
            
            _progressTracker.Initialize();
            
            _musicSelectionPageModel.SkipAllowed = true;
            
            _headerText.SetActive(true);
            
            _levelEditorPageModel.StateChanged += OnStateChanged;
        }

        protected override void OnHide()
        {
            base.OnHide();
            
            _progressTracker.CleanUp();
            
            _musicSelectionPageModel.SkipAllowed = false;
            
            _headerText.SetActive(false);
            
            _levelEditorPageModel.StateChanged -= OnStateChanged;
        }

        protected override void BeforeCleanUp()
        {
            base.BeforeCleanUp();

            _progress.CleanUp();
        }
        
        private void OnStateChanged(LevelEditorState state)
        {
            OpenSetLocationPanelIfNeeded(state);
            UpdateHeaderState(state);
        }

        private void OpenSetLocationPanelIfNeeded(LevelEditorState state)
        {
            if (state != LevelEditorState.TemplateSetup) return;
            if (!IsFirstOpen) return;
            _levelEditorPageModel.OnSetLocationsButtonClicked();
            IsFirstOpen = false;
        }

        private void UpdateHeaderState(LevelEditorState state)
        {
            _headerText.SetActive(state == LevelEditorState.TemplateSetup 
                               || (state == LevelEditorState.PurchasableAssetSelection && _levelEditorPageModel.CurrentAssetType == DbModelType.SetLocation));
        }
    }
}