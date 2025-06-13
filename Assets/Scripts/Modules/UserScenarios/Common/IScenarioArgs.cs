using System;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.Tasks;
using Bridge.Models.ClientServer.Template;
using Bridge.Models.Common;
using Bridge.Models.VideoServer;
using Common.Publishers;
using Models;
using Navigation.Args;
using Navigation.Core;

namespace Modules.UserScenarios.Common
{
    internal interface IScenarioArgs
    {
        PageId ExecutedFrom { get; }
    }

    public abstract class ScenarioArgsBase : IScenarioArgs
    {
        public PageId ExecutedFrom { get; internal set; }
    }

    public abstract class LevelCreationScenarioArgs : ScenarioArgsBase
    {
        internal Action<VideoMessageOpenArgs> StartVideoMessageEditorAction;
    }

    internal sealed class VideoMessageOpenArgs
    {
        public ShareDestination ShareDestination;
        public ChatInfo ChatInfo;
    }

    internal sealed class CreateNewLevelScenarioArgs : LevelCreationScenarioArgs
    {
        public HashtagInfo Hashtag;
        public IPlayableMusic Music;
        public ChatInfo OpenedFromChat;
        public ShareDestination ShareDestination;
        public bool ShowTemplateCreationStep;
        public bool ShowDressingStep;
        public Universe Universe;
    }

    public sealed class CreateNewLevelBasedOnTemplateScenarioArgs : LevelCreationScenarioArgs
    {
        public Guid RecommendationId;
        public long? SocialActionId;
        public TemplateInfo Template;
        public Action OnTemplateLoaded;
        public Action OnCancelCallback;
        public bool ShowGridPage;
        public Universe Universe;
    }

    internal sealed class EditDraftScenarioArgs : LevelCreationScenarioArgs
    {
        public Level Draft;
    }

    internal sealed class EditTaskDraftScenarioArgs : LevelCreationScenarioArgs
    {
        public Level Draft;
        public long TaskId;
    }

    internal sealed class EditLocalSavedLevelScenarioArgs : LevelCreationScenarioArgs
    {
        public Level LocallySavedLevel;
        public Level OriginalFromServer;
    }

    internal sealed class RemixLevelScenarioArgs : LevelCreationScenarioArgs
    {
        public Level Level;
        public long VideoId;
        public Action OnRemixCanceled;
        public Universe Universe;
        public long? InitialTemplateId { get; set; }
    }

    internal sealed class VideoMessageCreationScenarioArgs : ScenarioArgsBase
    {
        public Universe Universe;
        public ChatInfo OpenedFromChat;
        public PublishingType? PublishingType;
        public Action<StartLevelCreationArgs> StartLevelCreationAction;
        public ShareDestination ShareDestination;
    }

    internal sealed class StartLevelCreationArgs
    {
        public ShareDestination ShareDestination;
        public ChatInfo ChatInfo;
    }

    internal sealed class RemixLevelSocialActionScenarioArgs : LevelCreationScenarioArgs
    {
        public Guid RecommendationId;
        public long ActionId;
        public Level Level;
        public long VideoId;
        public Action OnRemixCanceled;
        public CharacterFullInfo[] Characters;
    }

    internal sealed class TaskScenarioArgs : ScenarioArgsBase
    {
        public TaskFullInfo TaskInfo;
        public Universe Universe;
    }

    internal sealed class CreateNewCharacterArgs : ScenarioArgsBase
    {
        public long? RaceId;
        public Action OnDisplayed;
    }

    internal sealed class EditCharacterArgs : ScenarioArgsBase
    {
        public CharacterFullInfo Character;
        public long? CategoryTypeId;
        public long? ThemeId;
    }

    internal sealed class OnboardingArgs : ScenarioArgsBase
    {
        public bool StartFromSignIn;
    }

    internal sealed class VotingFeedArgs : ScenarioArgsBase
    {
        public TaskFullInfo TaskInfo;
        public long? ActionId;
        public Guid RecommendationId;
        public Universe Universe;
    }

    internal sealed class NonLevelVideoUploadArgs : ScenarioArgsBase
    {
        public NonLeveVideoData NonLeveVideoData;
        public ChatInfo OpenChatOnComplete;
    }

    internal sealed class EditNameArgs : ScenarioArgsBase
    {
         
    }
}