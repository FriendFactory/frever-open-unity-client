using System;
using Common;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.LevelManagement;
using Navigation.Args;
using UIManaging.Pages.LevelEditor.Ui.Common;
using UIManaging.Pages.LevelEditor.Ui.PostRecordEditor;
using UIManaging.SnackBarSystem;
using static UIManaging.Pages.LevelEditor.Ui.PostRecordEditor.PostRecordEditorState;

namespace UIManaging.Pages.LevelEditor.Ui
{
    [UsedImplicitly]
    internal sealed class PostRecordEditorPageModel : BaseEditorPageModel
    {
        private readonly SnackBarHelper _snackBarHelper;
        
        //---------------------------------------------------------------------
        // Properties
        //--------------------------------------------------------------------- 

        public EventsTimelineModel PostRecordEventsTimelineModel { get; }
        public Level OriginalPostRecordLevel { get; set; }
        public Action OnMovingForward { get; set; }
        public bool CheckIfLevelWasModifiedBeforeExit { get; set; }
        public Action<MovingBackData> OnMovingBack { get; set; }
        private PostRecordEditorState PrevState { get; set; }
        public PostRecordEditorState CurrentState { get; private set; }

        //---------------------------------------------------------------------
        // Events
        //---------------------------------------------------------------------

        public event Action PostRecordEditorOpened;
        public event Action PostRecordEditorClosed;

        public event Action<PostRecordEditorState> StateChanged;

        public event Action OpenLevelAudioPanelClicked;
        public event Action LevelAudioPanelOpened;
        public event Action LevelAudioPanelClosed;

        public event Action AssetSelectionViewOpened;
        public event Action AssetSelectionViewClosed;

        public event Action CaptionCreationRequested;
        public event Action CaptionPanelClosed;

        public event Action CreateNewEventRequested;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        internal PostRecordEditorPageModel(ILevelManager levelManager, IOutfitFeatureControl outfitFeatureControl, SnackBarHelper snackBarHelper) : base(levelManager, outfitFeatureControl)
        {
            _snackBarHelper = snackBarHelper;
            IsPostRecordEditorOpened = true;
            PostRecordEventsTimelineModel = new EventsTimelineModel(levelManager);
            PrevState = None;
            CurrentState = Default;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void OpenPostRecordEditor()
        {
            IsPostRecordEditorOpened = true;
            PostRecordEditorOpened?.Invoke();
            InitializePostRecordingTimeline();
        }

        public void ClosePostRecordEditor()
        {
            PostRecordEditorClosed?.Invoke();
        }

        public void ChangeState(PostRecordEditorState state)
        {
            PrevState = CurrentState;
            CurrentState = state;

            StateChanged?.Invoke(state);
        }

        public void ReturnToPrevState()
        {
            ChangeState(PrevState);
        }

        public void OnOpenLevelAudioPanelClicked()
        {
            OpenLevelAudioPanelClicked?.Invoke();
        }

        public void OnLevelAudioPanelOpened()
        {
            LevelAudioPanelOpened?.Invoke();
            ChangeState(VolumeSettings);
        }
        
        public void OnLevelAudioPanelClosed()
        {
            LevelAudioPanelClosed?.Invoke();
            ChangeState(Default);
        }

        public void OnAssetSelectionViewOpened(bool isPurchasable = false)
        {
            AssetSelectionViewOpened?.Invoke();
            ChangeState(isPurchasable ? PurchasableAssetSelection : AssetSelection);
        }

        public void OnAssetSelectionViewClosed()
        {
            AssetSelectionViewClosed?.Invoke();
            ChangeState(Default);
        }

        public override void OnShoppingCartOpened()
        {
            ChangeState(PostRecordEditorState.ShoppingCart);
        }

        public override void OnShoppingCartClosed()
        {
            ReturnToPrevState();
        }

        public void TryToOpenNewCaptionAddingPanel()
        {
            var captionsCount = LevelManager.TargetEvent.GetCaptionsCount();
            if (captionsCount >= Constants.Captions.CAPTIONS_PER_EVENT_MAX)
            {
                _snackBarHelper.ShowFailSnackBar(Constants.Captions.REACHED_LIMIT_MESSAGE);
                return;
            }
            ChangeState(AssetSelection);
            CaptionCreationRequested?.Invoke();
        }
        
        public void OnCaptionPanelClosed()
        {
            CaptionPanelClosed?.Invoke();
        }

        public void RequestCreationOfNewEvent()
        {
            CreateNewEventRequested?.Invoke();
        }

        public override void OnChangeCameraAngleClicked()
        {
            ChangeState(CameraAngle);
            base.OnChangeCameraAngleClicked();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnSongSelectionOpened()
        {
            base.OnSongSelectionOpened();
            ChangeState(SongSelection);
        }

        protected override void OnSongSelectionCloseRequested()
        {
            base.OnSongSelectionCloseRequested();
            ChangeState(Default);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void InitializePostRecordingTimeline()
        {
            PostRecordEventsTimelineModel.Initialize();
            PostRecordEventsTimelineModel.SelectTargetEvent();
        }
    }
}