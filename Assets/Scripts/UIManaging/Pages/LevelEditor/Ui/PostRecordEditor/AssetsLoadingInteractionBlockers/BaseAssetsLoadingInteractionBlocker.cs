using Bridge.Models.ClientServer.Assets;
using Bridge.Models.Common;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Assets;
using Modules.LevelManaging.Editing.LevelManagement;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.AssetsLoadingInteractionBlockers
{
    internal abstract class BaseAssetsLoadingInteractionBlocker<T> : MonoBehaviour
    {
        [SerializeField] protected T[] _targets;

        private PostRecordEditorPageModel _editorPageModel;
        private ILevelManager _levelManager;

        private bool _isLoadingEvent;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject]
        [UsedImplicitly]
        private void Construct(PostRecordEditorPageModel editorPageModel, ILevelManager levelManager)
        {
            _editorPageModel = editorPageModel;
            _levelManager = levelManager;
        }

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected virtual void OnEnable()
        {
            SubscribeToAssetChangeEvents();
            RefreshButtonInteractivity();
        }

        protected virtual void OnDisable()
        {
            UnsubscribeFromAssetChangeEvents();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected abstract void RefreshInteractivity(bool isInteractable);

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void SubscribeToAssetChangeEvents()
        {
            if (_levelManager == null) return;

            _levelManager.AssetLoaded += OnAssetLoaded;
            _levelManager.EventLoadingStarted += OnEventLoadingStarted;
            _levelManager.EventLoadingCompleted += OnEventLoadingCompleted;
            _levelManager.AssetUpdateStarted += AssetStartedChanging;
            _levelManager.AssetUpdateCancelled += OnAssetUpdateCancelled;
            _levelManager.AssetUpdateCompleted += OnAssetUpdated;
            _levelManager.AssetUpdateFailed += OnAssetUpdatingFailed;
            _levelManager.SetLocationChangeFinished += OnSetLocationChangeFinished;
            _levelManager.CharacterReplacementStarted += OnCharacterReplacementStarted;
            _levelManager.CharacterReplaced += OnCharacterReplaced;
            _levelManager.CharactersOutfitsUpdated += OnCharactersOutfitsUpdated;
        }

        private void UnsubscribeFromAssetChangeEvents()
        {
            if (_levelManager == null) return;
            
            _levelManager.AssetLoaded -= OnAssetLoaded;
            _levelManager.EventLoadingStarted -= OnEventLoadingStarted;
            _levelManager.EventLoadingCompleted -= OnEventLoadingCompleted;
            _levelManager.AssetUpdateStarted -= AssetStartedChanging;
            _levelManager.AssetUpdateCompleted -= OnAssetUpdated;
            _levelManager.AssetUpdateCancelled -= OnAssetUpdateCancelled;
            _levelManager.AssetUpdateFailed -= OnAssetUpdatingFailed;
            _levelManager.SetLocationChangeFinished -= OnSetLocationChangeFinished;
            _levelManager.CharacterReplacementStarted -= OnCharacterReplacementStarted;
            _levelManager.CharacterReplaced -= OnCharacterReplaced;
            _levelManager.CharactersOutfitsUpdated -= OnCharactersOutfitsUpdated;
        }

        private void OnEventLoadingStarted()
        {
            _isLoadingEvent = true;
            RefreshButtonInteractivity();
        }
        
        private void OnEventLoadingCompleted()
        {
            _isLoadingEvent = false;
            RefreshButtonInteractivity();
        }

        private void RefreshButtonInteractivity()
        {
            if (_levelManager == null) return;     
            
            var currentAssetType = _editorPageModel.CurrentAssetType;
            var isLoadingCharacter = currentAssetType == DbModelType.Character && _levelManager.IsReplacingCharacter;
            var isLoadingAssets = _levelManager.IsChangingAsset;
            var isLoadingAssetOfCurrentType = _levelManager.IsLoadingAssetsOfType(currentAssetType);
            var isInteractable = !isLoadingCharacter && !isLoadingAssets && !isLoadingAssetOfCurrentType && !_isLoadingEvent;
            
            RefreshInteractivity(isInteractable);
        }

        private void OnAssetLoaded(IEntity model)
        {
            RefreshButtonInteractivity();
        }
        
        private void AssetStartedChanging(DbModelType type, long id)
        {
            RefreshButtonInteractivity();
        }

        private void OnAssetUpdateCancelled(DbModelType type)
        {
            RefreshButtonInteractivity();
        }
        
        private void OnAssetUpdated(DbModelType type)
        {
            RefreshButtonInteractivity();
        }

        private void OnAssetUpdatingFailed(DbModelType type)
        {
            RefreshButtonInteractivity();
        }

        private void OnCharacterReplacementStarted(CharacterFullInfo oldCharacter, CharacterFullInfo newCharacter)
        {
            RefreshButtonInteractivity();
        }
        
        private void OnSetLocationChangeFinished(ISetLocationAsset setLocationAsset)
        {
            RefreshButtonInteractivity(); 
        }
        
        private void OnCharacterReplaced(ICharacterAsset characterAsset)
        {
            RefreshButtonInteractivity();
        }

        private void OnCharactersOutfitsUpdated()
        {
            RefreshButtonInteractivity();
        }
    }
}
