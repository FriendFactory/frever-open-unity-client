using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.LevelManagement;
using Navigation.Args;
using UIManaging.Core;
using UIManaging.Localization;
using UIManaging.Pages.LevelEditor.EditingPage;
using UnityEngine;
using Zenject;
using IInitializable = Abstract.IInitializable;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor
{
    internal sealed class OpenPostRecordEditorButton : ButtonBase, IInitializable
    {
        [SerializeField] private EditingPageLoading _pageLoading;

        [Inject] private LoadingOverlayLocalization _loadingOverlayLocalization;
        
        private LevelEditorPageModel _levelEditorPageModel;
        private ILevelManager _levelManager;
        
        public bool IsInitialized { get; private set; }

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        [Inject]
        [UsedImplicitly]
        private void Construct(LevelEditorPageModel levelEditorPageModel, ILevelManager levelManager)
        {
            _levelEditorPageModel = levelEditorPageModel;
            _levelManager = levelManager;
        }
        
        //---------------------------------------------------------------------
        // IInitalizable
        //---------------------------------------------------------------------

        public void Initialize()
        {
            _levelManager.EventDeleted += RefreshInteractivity;
            _levelManager.RecordingCancelled += RefreshInteractivity;
            _levelManager.EventSaved += RefreshInteractivity;
            _levelManager.LevelPreviewCompleted += RefreshInteractivity;
            _levelManager.PreviewCancelled += RefreshInteractivity;
            _levelManager.CurrentLevelChanged += RefreshInteractivity;
            
            _levelManager.AssetUpdateStarted += OnAssetUpdateStarted;
            _levelManager.AssetUpdateCompleted += OnAssetUpdateCompleted;
            _levelManager.AssetUpdateFailed += OnAssetUpdateCompleted;
            
            _levelManager.RecordingStarted += OnRecordingStarted;
            _levelManager.EventDeletionStarted += OnEventDeletionStarted;

            _levelManager.CharacterSpawnStarted += OnCharacterSpawnStarted;
            _levelManager.CharacterSpawned += OnCharacterSpawned;
            
            RefreshInteractivity();
            
            IsInitialized = true;
        }

        public void CleanUp()
        {
            _levelManager.EventDeleted -= RefreshInteractivity;
            _levelManager.RecordingCancelled -= RefreshInteractivity;
            _levelManager.EventSaved -= RefreshInteractivity;
            _levelManager.LevelPreviewCompleted -= RefreshInteractivity;
            _levelManager.PreviewCancelled -= RefreshInteractivity;
            _levelManager.CurrentLevelChanged -= RefreshInteractivity;
            
            _levelManager.RecordingStarted -= OnRecordingStarted;
            _levelManager.EventDeletionStarted -= OnEventDeletionStarted;

            _levelManager.AssetUpdateStarted -= OnAssetUpdateStarted;
            _levelManager.AssetUpdateCompleted -= OnAssetUpdateCompleted;
            _levelManager.AssetUpdateFailed -= OnAssetUpdateCompleted;

            _levelManager.CharacterSpawnStarted -= OnCharacterSpawnStarted;
            _levelManager.CharacterSpawned -= OnCharacterSpawned;
            
            IsInitialized = false;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnClick()
        {
            var message = _loadingOverlayLocalization.CollectingClipsHeader;
            _pageLoading.ShowDark(message, false, true);
            
            _levelManager.CleanUp();
            _levelEditorPageModel.OnMoveToPostRecordEditorClicked();
            var args = new MovingForwardArgs()
            {
                NavigationMessage = message
            };
            _levelEditorPageModel.OpeningPageArgs.OnMovingForwardRequested?.Invoke(args);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnRecordingStarted()
        {
            Interactable = false;
        }
        
        private void OnEventDeletionStarted()
        {
            Interactable = false;
        }

        private void OnAssetUpdateStarted(DbModelType type, long id)
        {
            if (type != DbModelType.Outfit) return;
            Interactable = false;
        }

        private void OnAssetUpdateCompleted(DbModelType type)
        {
            if (type != DbModelType.Outfit) return;
            RefreshInteractivity();
        }

        private void OnCharacterSpawnStarted()
        {
            Interactable = false;
        }

        private void OnCharacterSpawned(Modules.LevelManaging.Assets.ICharacterAsset obj)
        {
            RefreshInteractivity();
        }

        private void RefreshInteractivity()
        {
            var isActive = _levelManager.CurrentLevel != null && !_levelManager.IsLevelEmpty && !_levelManager.IsLoadingAssets();
            Interactable = isActive;
            
            gameObject.SetActive(isActive);
        }
    }
}
