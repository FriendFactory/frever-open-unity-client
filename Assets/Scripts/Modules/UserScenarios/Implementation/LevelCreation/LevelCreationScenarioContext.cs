using System;
using System.Collections.Generic;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.ClientServer.EditorsSetting;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.Tasks;
using Bridge.Models.Common;
using Bridge.Models.VideoServer;
using Common.Publishers;
using Models;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Implementation.LevelCreation
{
    internal interface ILevelCreationScenarioContext : ITasksExitContext
    {
        Guid RecommendationId { get; set; }
        long? SocialActionId { get; set; }
        Level LevelData { get; set; }
        Level OriginalLevelData { get; set; }
        Level LevelToStartOver { get; set; }
        long? TaskId { get; }
        long? InitialTemplateId { get; set; }
        long? VideoId { get; set; }
        Action OnLevelCreationCanceled { get; set; }
        HashtagInfo Hashtag { get; set; }
        bool SaveEventThumbnails { get; set; }
        string NavigationMessage { get; set; }
        Action OnClearPrivacyData { get; set; }
        ChatInfo OpenedFromChat { get; set; }
        bool SavedAsDraft { get; set; }
        IPlayableMusic Music { get; set; }

        LevelEditorContext LevelEditor { get; set; }
        PostRecordEditorContext PostRecordEditor { get; set; }
        CharacterEditorContext CharacterEditor { get; set; }
        CharacterSelectionContext CharacterSelection { get; set; }
        PublishContext PublishContext { get; set; }
        UploadContext UploadContext { get; set; }
        
        Action<VideoMessageOpenArgs> ExecuteVideoMessageCreationScenario { get; set; }
        Action<StartLevelCreationArgs> ExecuteLevelCreationScenario { get; set; }
    }
    
    internal sealed class LevelCreationScenarioContext: ILevelCreationScenarioContext
    {
        public Guid RecommendationId { get; set; }
        public long? SocialActionId { get; set; }
        public Level LevelData { get; set; }
        public Level OriginalLevelData { get; set; }
        public Level LevelToStartOver { get; set; }
        public long? InitialTemplateId { get; set; }
        public long? VideoId { get; set; }
        public Action OnLevelCreationCanceled { get; set; }
        public HashtagInfo Hashtag { get; set; }
        public bool SaveEventThumbnails { get; set; }
        public string NavigationMessage { get; set; }
        public Action OnClearPrivacyData { get; set; }
        public long? TaskId => Task?.Id;
        public IPlayableMusic Music { get; set; }
        
        public PageId OpenedFromPage { get; set; }
        public ChatInfo OpenedFromChat { get; set; }
        public TaskFullInfo Task { get; set; }
        public bool SavedAsDraft { get; set; }

        public LevelEditorContext LevelEditor { get; set; } = new();
        public PostRecordEditorContext PostRecordEditor { get; set; } = new();
        public CharacterEditorContext CharacterEditor { get; set; } = new();
        public CharacterSelectionContext CharacterSelection { get; set; } = new();
        public PublishContext PublishContext { get; set; } = new();
        public UploadContext UploadContext { get; set; } = new();
       
        public Action<VideoMessageOpenArgs> ExecuteVideoMessageCreationScenario { get; set; }
        public Action<StartLevelCreationArgs> ExecuteLevelCreationScenario { get; set; }
    }

    internal class LevelEditorContext
    {
        public bool OpenVideoUploadMenu;
        public bool NewEventsDeletionOnly;
        public bool LinkTemplateToEvent;
        public long? TemplateId;
        public ExitButtonBehaviour ExitButtonBehaviour;
        public DraftEventData DraftEventData;
        public Dictionary<long, CharacterFullInfo> CharactersToUseInTemplate;
        public Action OnLevelEditorLoaded;
        public LevelEditorSettings LevelEditorSettings;
        public ScenarioState OnMoveBack;
        public bool ShowTaskInfo;
        public bool ShowLoadingPagePopup;
        public bool ShowTemplateCreationStep;
        public bool ShowDressingStep;
    }

    internal class PostRecordEditorContext
    {
        public bool IsPreviewMode;
        public bool ShowPageLoadingPopup;
        public bool DecompressBundlesAfterPreview;
        public bool ShowTaskInfo;
        public bool CheckIfUserMadeEnoughChangesForTask;
        public bool CheckIfLevelWasModifiedBeforeExit;
        public PostRecordEditorSettings PostRecordEditorSettings;
        public OpeningState OpeningPipState;
    }

    internal class CharacterEditorContext
    {
        public CharacterFullInfo Character;
        public OutfitFullInfo Outfit;
        public CharacterEditorSettings CharacterEditorSettings;
        public ScenarioState OpenedFrom;
        public bool ShowTaskInfo;
    }

    internal class CharacterSelectionContext
    {
        public string Header;
        public string HeaderDescription;
        public string ReasonText;
        public long[] AutoPickedCharacters;
        public long[] CharacterToReplaceIds;
        public Race Race;
        public Dictionary<long, CharacterFullInfo> PickedCharacters;
    }

    public class PublishContext
    {
        public PublishingType PublishingType;
        public string PortraitVideoFilePath;
        public VideoUploadingSettings VideoPublishSettings;
        public NonLeveVideoData NonLevelVideoData;
    }

    public class UploadContext
    {
        public bool UploadOnExit = true;
        public Video Video;
    }
}