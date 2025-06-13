using System;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.EditorsSetting;
using Bridge.Models.ClientServer.Tasks;
using Bridge.Models.ClientServer.Template;
using Bridge.Models.Common;
using Bridge.Models.VideoServer;
using Extensions;
using Models;
using Modules.CameraSystem.CameraAnimations;
using Modules.LevelManaging.Editing.Templates;
using Navigation.Core;

namespace Navigation.Args
{
    public sealed class LevelEditorArgs : PageArgs
    {
        public Level LevelData;
        public Level OriginalLevelData;
        public Level LevelToStartOver;
        public DraftEventData DraftEventData;
        public string NavigationMessage;
        public TemplateInfo Template;
        public ReplaceCharacterData[] ReplaceCharactersData;
        public bool LinkTemplateToEvent = true;
        public bool OpenVideoUploadMenu;
        public bool NewEventsDeletionOnly;
        public bool ShowTaskInfo;
        public bool ShowTemplateCreationStep;
        public bool ShowDressingStep;
        public TaskFullInfo TaskFullInfo;
        public ExitButtonBehaviour ExitButtonBehaviour;
        public HashtagInfo TemplateHashtagInfo { get; set; }
        public IPlayableMusic Music { get; set; }

        public Action CancelLoadingAction;
        public Action HideLoadingPopupRequested;
        public Action<MovingForwardArgs> OnMovingForwardRequested;
        public Action OnLevelEditorLoaded;
        public Action<LevelEditorExitArgs> OnExitRequested;
        public Action<NonLeveVideoData> OnOpenVideoUploadPageRequested;
        public Action OnStartOverRequested;
        public Action<LevelEditorCreationOutfitReqData> OnOutfitCreationRequested;
        public Action OnCreateVideoMessageRequested;

        public LevelEditorSettings Settings;
        public override PageId TargetPage => PageId.LevelEditor;
        public long? TaskId => TaskFullInfo?.Id;

        public override object Clone()
        {
            var clone = new LevelEditorArgs
            {
                LevelData = LevelData?.Clone(),
                OriginalLevelData = OriginalLevelData?.Clone(),
                LevelToStartOver = LevelToStartOver.Clone(),
                Template = Template,
                TaskFullInfo = TaskFullInfo,
                ExitButtonBehaviour = ExitButtonBehaviour,
                LinkTemplateToEvent = LinkTemplateToEvent,
                NavigationMessage = NavigationMessage,
                CancelLoadingAction = CancelLoadingAction,
                Backed = Backed,
                ReplaceCharactersData = ReplaceCharactersData,
                OnMovingForwardRequested = OnMovingForwardRequested,
                OnLevelEditorLoaded = OnLevelEditorLoaded,
                OnExitRequested = OnExitRequested,
                OnOpenVideoUploadPageRequested = OnOpenVideoUploadPageRequested,
                OpenVideoUploadMenu = OpenVideoUploadMenu,
                NewEventsDeletionOnly = NewEventsDeletionOnly,
                OnStartOverRequested = OnStartOverRequested,
                Settings = Settings,
                ShowHintsOnDisplay = ShowHintsOnDisplay,
                ShowTaskInfo = ShowTaskInfo,
                Music = Music,
            };
            return clone;
        }
    }

    public struct LevelEditorExitArgs
    {
        public bool SavedToDraft;
    }
    
    public sealed class MovingForwardArgs
    {
        public string NavigationMessage;
    }

    public sealed class LevelEditorCreationOutfitReqData
    {
        public CharacterFullInfo TargetCharacter;
        public OutfitFullInfo CurrentOutfit;
        public Level Level;
        public DraftEventData DraftEventData;
    }

    public struct DraftEventData
    {
        public Event Event;
        public CameraAnimationFrame CameraPosition;
    }

    public struct NonLeveVideoData
    {
        public string Path;
        public int DurationSec;
        public bool AllowComment;
    }

    public enum ExitButtonBehaviour
    {
        StartOverMenu,
        DiscardingAllRecordMenu
    }
}
