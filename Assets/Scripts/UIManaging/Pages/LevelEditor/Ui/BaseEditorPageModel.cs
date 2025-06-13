using System;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.LevelManagement;
using UIManaging.Pages.Common.SongOption;
using UIManaging.Pages.LevelEditor.Ui.Common;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using Event = Models.Event;

namespace UIManaging.Pages.LevelEditor.Ui
{
    internal abstract class BaseEditorPageModel
    {
        private bool _createNewOutfitRequested;
        protected readonly ILevelManager LevelManager;
        protected readonly IMetadataProvider MetadataProvider;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public EventsTimelineModel PreviewEventsTimelineModel { get;}
        public Level OriginalEditorLevel { get; set; }
        public long SwitchTargetCharacterId { get; private set; }
        public DbModelType CurrentAssetType { get; private set; }
        public Event TargetEventOriginal { get; private set; }
        public bool IsPostRecordEditorOpened { get; protected set; }

        public Universe Universe { get; set; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action BodyAnimationsButtonClicked;
        public event Action VfxButtonClicked;
        public event Action VoiceButtonClicked;
        public event Action SetLocationsButtonClicked;
        public event Action CameraButtonClicked;
        public event Action OutfitPanelOpened;
        public event Action OutfitChangingBegun;
        public event Action<string> InformationMessageRequested;
        public event Action AnyAssetButtonClicked;

        public event Action PostRecordEditorRemoveCameraTexture;
        public event Action PostRecordEditorSetCameraTexture;
        public event Action SwitchCharacterButtonClicked;
        public event Action CharacterToSwitchClicked;
        public event Action<long> CharacterButtonClicked;
        public event Action<long> CharacterSwitchableButtonClicked;
        public event Action<VoiceFilterFullInfo> VoiceFilterClicked;
        public event Action<object> CharacterItemClicked;
        public event Action<Event> PostRecordEditorEventSelectionChanged;
        public event Action CameraFilterClicked;
        public event Action PostRecordEditorUnsubscribe;
        public event Action PostRecordEditorSubscribe;
        public event Action ChangeCameraAngleRequested;
        public event Action<string> ShowLoadingIconOverlay;
        public event Action ShowPageLoadingOverlayRequested;
        public event Action HideLoadingIconOverlay;
        public event Action SongSelectionOpened;
        public event Action SongSelectionClosed;
        public event Action CharacterLoaded;
        public event Action OutfitButtonPressed;
        public event Action<long, long?> CreateNewOutfitRequested;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        private readonly IOutfitFeatureControl _outfitFeatureControl;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        protected BaseEditorPageModel(ILevelManager levelManager, IOutfitFeatureControl outfitFeatureControl)
        {
            LevelManager = levelManager;
            _outfitFeatureControl = outfitFeatureControl;
            PreviewEventsTimelineModel = new EventsTimelineModel(LevelManager);
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OnBodyAnimationsButtonClicked()
        {
            CurrentAssetType = DbModelType.BodyAnimation;
            BodyAnimationsButtonClicked?.Invoke();
            OnAnyAssetButtonClicked();
        }

        public void OnVfxButtonClicked()
        {
            CurrentAssetType = DbModelType.Vfx;
            VfxButtonClicked?.Invoke();
            OnAnyAssetButtonClicked();
        }

        public void OnSetLocationsButtonClicked()
        {
            CurrentAssetType = DbModelType.SetLocation;
            SetLocationsButtonClicked?.Invoke();
            OnAnyAssetButtonClicked();
        }
        
        public void OnVoiceButtonClicked()
        {
            CurrentAssetType = DbModelType.VoiceFilter;
            VoiceButtonClicked?.Invoke();
            OnAnyAssetButtonClicked();
        }

        public void OnCameraButtonClicked()
        {
            CurrentAssetType = DbModelType.CameraAnimation;
            CameraButtonClicked?.Invoke();
            OnAnyAssetButtonClicked();
        }

        public void OnOutfitButtonPressed()
        {
            OutfitButtonPressed?.Invoke();
        }

        public void OpenOutfitPanel()
        {
            CurrentAssetType = DbModelType.Outfit;
            OutfitPanelOpened?.Invoke();
            OnAnyAssetButtonClicked();
        }

        public void ShowInformationMessage(string message)
        {
            InformationMessageRequested?.Invoke(message);
        }

        public void OnSwitchCharacterButtonClicked()
        {
            CurrentAssetType = DbModelType.Character;
            UpdateSwitchTargetCharacterId();
            SwitchCharacterButtonClicked?.Invoke();
            OnAnyAssetButtonClicked();
        }

        public void OnFilterClicked()
        {
            CurrentAssetType = DbModelType.CameraFilterVariant;
            CameraFilterClicked?.Invoke();
            OnAnyAssetButtonClicked();
        }

        private void OnAnyAssetButtonClicked()
        {
            TargetEventOriginal = LevelManager.TargetEvent.Clone();
            AnyAssetButtonClicked?.Invoke();
        }
        
        public void OnCharacterButtonClicked(long characterId)
        {
            CharacterButtonClicked?.Invoke(characterId);
        }
        
        public void OnCharacterSwitchableButtonClicked(long characterId)
        {
            SetSwitchTargetCharacterId(characterId);
            CharacterSwitchableButtonClicked?.Invoke(characterId);
        }
        
        public void OnCharacterItemClicked(object character)
        {
            CharacterItemClicked?.Invoke(character);
        }

        public void PostRecordEditorUnsubscribeFromEvents()
        {
            PostRecordEditorUnsubscribe?.Invoke();
        }
        
        public void PostRecordEditorSubscribeToEvents()
        {
            PostRecordEditorSubscribe?.Invoke(); 
        }
        
        public virtual void OnChangeCameraAngleClicked()
        {
            ChangeCameraAngleRequested?.Invoke();
        }
        
        public void SetPostRecordEditorCameraTexture()
        {
            PostRecordEditorSetCameraTexture?.Invoke();
        }

        public void RemovePostRecordEditorCameraTexture()
        {
            PostRecordEditorRemoveCameraTexture?.Invoke();
        }

        public void InitializePreviewTimeline()
        {
            PreviewEventsTimelineModel.Initialize();
            PreviewEventsTimelineModel.SelectTargetEvent();
        }

        public void SetSwitchTargetCharacterId(long characterId)
        {
            SwitchTargetCharacterId = characterId;
        }

        public void OnVoiceFilterClicked(VoiceFilterFullInfo voiceFilter)
        {
            VoiceFilterClicked?.Invoke(voiceFilter);
        }

        public void OnPostRecordEditorEventSelectionChanged(Event ev)
        {
            PostRecordEditorEventSelectionChanged?.Invoke(ev);
        }

        public void OnOutfitChangingBegun()
        {
            OutfitChangingBegun?.Invoke();
        }

        public void ShowLoadingOverlay(string title = "") 
        {
            ShowLoadingIconOverlay?.Invoke(title);
        }

        public void ShowPageLoadingOverlay()
        {
            ShowPageLoadingOverlayRequested?.Invoke();
        }

        public void HideLoadingOverlay()
        {
            HideLoadingIconOverlay?.Invoke();
        }

        public void ConfirmCharacterLoaded()
        {
            CharacterLoaded?.Invoke();
        }

        public void SetupSongSelection(MusicSelectionPageModel  musicSelectionPageModel)
        {
            musicSelectionPageModel.PageOpened += OnSongSelectionOpened;
            musicSelectionPageModel.PageCloseRequested += OnSongSelectionCloseRequested;
        }

        public abstract void OnShoppingCartOpened();

        public abstract void OnShoppingCartClosed();
        
        public void OnCreateNewOutfitClicked(long categoryId, long subcategoryId)
        {
            if (_createNewOutfitRequested || !CanCreateNewOutfitForTargetCharacter()) return;
            _createNewOutfitRequested = true;
            LevelManager.WarmupUmaBundlesForWardrobesModification();
            CreateNewOutfitRequested?.Invoke(categoryId, subcategoryId);   
        }
        
        public virtual bool CanChangeOutfitForTargetCharacter(ref string message)
        {
            return _outfitFeatureControl.CanChangeOutfitForTargetCharacter(ref message);
        }
        
        public virtual bool CanChangeOutfitForTargetCharacter()
        {
            return _outfitFeatureControl.CanChangeOutfitForTargetCharacter();
        }

        public virtual bool CanCreateNewOutfitForTargetCharacter()
        {
            return _outfitFeatureControl.CreationOutfitAllowed && CanChangeOutfitForTargetCharacter() &&
                   !LevelManager.IsChangingOutfit && !LevelManager.IsLoadingAssets();
        }

        public void EnableOutfitChange()
        {
            _createNewOutfitRequested = false;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected virtual void OnSongSelectionOpened()
        {
            SongSelectionOpened?.Invoke();
        }

        protected virtual void OnSongSelectionCloseRequested()
        {
            SongSelectionClosed?.Invoke();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateSwitchTargetCharacterId()
        {
            var characterToSwitch = LevelManager.TargetEvent.IsGroupFocus()
                ? LevelManager.TargetEvent.GetOrderedCharacterControllers().First().CharacterId
                : LevelManager.TargetEvent.GetTargetCharacterController().CharacterId;
            SetSwitchTargetCharacterId(characterToSwitch);
        }

        public void OnCharacterToSwitchClicked()
        {
            CharacterToSwitchClicked?.Invoke();
        }
    }
}