using System;
using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Assets;
using Bridge.Models.ClientServer.Chat;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Bridge.Models.ClientServer.Tasks;
using Bridge.Models.Common;
using Bridge.Models.VideoServer;
using Bridge.Services.SelfieAvatar;
using Models;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.CharacterCreation;
using Modules.UserScenarios.Implementation.LevelCreation;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Common;
using UIManaging.Common.Interfaces;
using UIManaging.Pages.VotingFeed.Interfaces;
using UnityEngine;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace Modules.UserScenarios.Implementation.VotingFeed
{
    internal sealed class VotingFeedContext: ILevelCreationScenarioContext, ICharacterCreationContext
    {
        public Guid RecommendationId { get; set; }
        public long? SocialActionId { get; set; }
        public bool IsNewCharacter { get; set; }
        public CharacterFullInfo Character { get; set; }
        public CharacterInfo Style { get; set; }
        public Race Race { get; set; }
        public Gender Gender { get; set; }
        public JSONSelfie? JsonSelfie { get; set; }
        public Dictionary<CharacterInfo, Sprite> CharacterStyles { get; set; }
        public bool AllowBackFromGenderSelection { get; set; }
        public CreateMode? SelectedCreateMode { get; set; }
        public long? CategoryTypeId { get; set; }
        public long? ThemeCollectionId { get; set; }
        public bool RaceLocked { get; set; }

        public Level LevelData { get; set; }
        public Level OriginalLevelData { get; set; }
        public Level LevelToStartOver { get; set; }
        public long? TaskId => Task?.Id;
        public long? InitialTemplateId { get; set; }
        public long? VideoId { get; set; }
        public Action OnLevelCreationCanceled { get; set; }
        public HashtagInfo Hashtag { get; set; }
        public bool SaveEventThumbnails { get; set; }
        public string NavigationMessage { get; set; }
        public Action OnClearPrivacyData { get; set; }
        public ChatInfo OpenedFromChat { get; set; }
        public bool SavedAsDraft { get; set; }

        public IPlayableMusic Music { get; set; }

        public PageId OpenedFromPage { get; set; }
        public TaskFullInfo Task { get; set; }
        public List<BattleData> AllBattleData { get; set; }

        public LevelEditorContext LevelEditor { get; set; } = new();
        public PostRecordEditorContext PostRecordEditor { get; set; } = new();
        public CharacterEditorContext CharacterEditor { get; set; } = new();
        public CharacterSelectionContext CharacterSelection { get; set; } = new();
        public PublishContext PublishContext { get; set; } = new();
        public UploadContext UploadContext { get; set; } = new();

        public Action<VideoMessageOpenArgs> ExecuteVideoMessageCreationScenario { get; set; }
        public Action<StartLevelCreationArgs> ExecuteLevelCreationScenario { get; set; }
        public Action OnDisplayed { get; set; }
    }
}