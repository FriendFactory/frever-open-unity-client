using System.Linq;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.EditorsSetting;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.EditorsCommon;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Navigation.Core;
using UIManaging.Common;
using UIManaging.Common.Interfaces;
using UIManaging.Pages.VotingFeed.Interfaces;
using UnityEngine;
using Zenject;

namespace Modules.UserScenarios.Implementation.VotingFeed
{
    [UsedImplicitly]
    internal sealed class VotingFeedScenario : ScenarioBase<VotingFeedArgs, VotingFeedContext>, IVotingFeedScenario
    {
        private const ScenarioState ENTRY_STATE = ScenarioState.StyleBattleStart;

        private readonly IEditorSettingsProvider _editorSettingsProvider;
        
        public VotingFeedScenario(DiContainer diContainer, IEditorSettingsProvider editorSettingsProvider) : base(diContainer)
        {
            _editorSettingsProvider = editorSettingsProvider;
        }
        
        protected override async Task<VotingFeedContext> SetupContext()
        {
            var settings = await GetSettings();
            var context = new VotingFeedContext
            {
                SocialActionId = InitialArgs.ActionId,
                RecommendationId = InitialArgs.RecommendationId,
                Task = InitialArgs.TaskInfo,
                PostRecordEditor =
                {
                    PostRecordEditorSettings = settings.PostRecordEditorSettings
                },
                LevelEditor =
                {
                    LevelEditorSettings = settings.LevelEditorSettings
                },
                CharacterEditor =
                {
                    CharacterEditorSettings = settings.CharacterEditorSettings
                }
            };

            return context;
        }

        private Task<EditorsSettings> GetSettings()
        {
            return _editorSettingsProvider.GetEditorSettingsForTask(InitialArgs.TaskInfo.Id);
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var setup = Resolve<VotingFeedStatesSetup>();
            return Task.FromResult(setup.States);
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs,Resolve<IBridge>(), Resolve<IMetadataProvider>(), ENTRY_STATE);
        }
        
        private sealed class EntryTransition: EntryTransitionBase<VotingFeedContext>
        {
            public override ScenarioState To { get; }
            
            private MetadataStartPack MetadataStartPack => _metadataProvider.MetadataStartPack;

            private readonly VotingFeedArgs _args;
            private readonly IBridge _bridge;
            private readonly IMetadataProvider _metadataProvider;

            public EntryTransition(VotingFeedArgs args, IBridge bridge, IMetadataProvider metadataProvider, ScenarioState to) : base(args)
            {
                _args = args;
                _bridge = bridge;
                To = to;
                _metadataProvider = metadataProvider;
            }
            
            protected override async Task UpdateContext()
            {
                Context.PostRecordEditor.CheckIfLevelWasModifiedBeforeExit = false;
                Context.CharacterSelection.Race = MetadataStartPack.GetRaceByUniverseId(_args.Universe.Id);
                
                if (!Context.Task.IsAvailableForVoting)
                {
                    await base.UpdateContext();
                    return;
                }
                
                var videosResult = await _bridge.GetVotingBattlePairs(Context.Task.Id);
                
                if (videosResult.IsError)
                {
                    Debug.LogError($"Failed to load voting battle pairs, reason: {videosResult.ErrorMessage}");
                    return;
                }

                Context.AllBattleData = videosResult.Models.Select(battle => new BattleData
                {
                    BattleId = battle.Id,
                    TaskName = battle.Videos.FirstOrDefault()?.Video.TaskName,
                    BattleVideos = battle.Videos.Select(battleVideo => new BattleVideoData
                    {
                        VideoModel = new PreloadedVideoModel(battleVideo.Video, battleVideo.FileUrl),
                        WinScore = battleVideo.WinScore,
                        LoseScore = battleVideo.LossScore
                    }).ToArray()
                }).ToList();

                foreach (var video in Context.AllBattleData.SelectMany(battle => 
                        battle.BattleVideos.Select(battleVideo => battleVideo.VideoModel)))
                {
                    video.LoadVideo(); // potentially load in sequence?
                }
                
                await base.UpdateContext();
            }
        }
    }
}