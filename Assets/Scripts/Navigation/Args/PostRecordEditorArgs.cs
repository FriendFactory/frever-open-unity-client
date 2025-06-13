using System;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.EditorsSetting;
using Extensions;
using Bridge.Models.ClientServer.Tasks;
using Models;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class PostRecordEditorArgs : PageArgs
    {
        public Level LevelData;
        public bool IsPreviewMode;
        public bool DecompressBundlesAfterPreview;
        public bool EnablePreviewCancellation = true;
        public Action OnLoadingCompleted;
        public Action<DbModelType> OnAssetSelectionClosed;
        public Action<DbModelType> OnAssetSelectionOpened;
        public Action OnPreviewCompleted;
        public Action OnMoveForwardRequested;
        public Action CancelLoadingAction;
        public Action<MovingBackData> OnMovingBackRequested;
        public Action<PiPOutfitCreationRequestData> OnOutfitCreationRequested;
        public Action<CreateNewEventRequestData> OnNewEventCreationRequested;
        public string NavigationMessage;
        public PostRecordEditorSettings Settings;
        public TaskFullInfo TaskFullInfo;
        public OpeningState OpeningState;
        public DbModelType? DefaultOpenedAssetSelector;
        public bool ShowTaskInfo;
        public bool CheckIfUserMadeEnoughChangesForTask;
        public bool CheckIfLevelWasModifiedBeforeExit;
        public Action RequestHideLoadingPopup;

        public long? TaskId => TaskFullInfo?.Id;
        public override PageId TargetPage => PageId.PostRecordEditor;
    }

    public struct OpeningState
    {
        public int TargetEventSequenceNumber;
    }

    public sealed class MovingBackData
    {
        public Level LevelData;
        public Level OriginalLevelData;
        public bool SavedAsDraft;
    }

    public sealed class PiPOutfitCreationRequestData
    {
        public CharacterFullInfo TargetCharacter;
        public OutfitFullInfo CurrentOutfit;
        public int TargetEventSequenceNumber;
    }

    public sealed class CreateNewEventRequestData
    {
        public long? TemplateId;
    }
}