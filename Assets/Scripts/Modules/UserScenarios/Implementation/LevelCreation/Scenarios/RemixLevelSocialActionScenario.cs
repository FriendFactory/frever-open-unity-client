using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using JetBrains.Annotations;
using Modules.EditorsCommon;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Modules.UserScenarios.Implementation.LevelCreation.NavigationSetups;
using UIManaging.Localization;
using Zenject;

namespace Modules.UserScenarios.Implementation.LevelCreation.Scenarios
{
    [UsedImplicitly]
    internal sealed class RemixLevelSocialActionScenario : LevelCreationScenarioBase<RemixLevelSocialActionScenarioArgs>, IRemixLevelSocialActionScenario
    {
        [Inject] private LoadingOverlayLocalization _loadingOverlayLocalization;
        
        public RemixLevelSocialActionScenario(DiContainer diContainer, IEditorSettingsProvider editorSettingsProvider, ILevelCreationStatesSetupProvider statesSetupProvider) : base(diContainer, editorSettingsProvider, statesSetupProvider)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs, Resolve<ILevelHelper>(), ScenarioState.PostRecordEditor, _loadingOverlayLocalization.RemixHeader);
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var setup = StatesSetupProvider.GetStatesSetup(LevelCreationSetup.RemixSocialAction);
            
            return Task.FromResult(setup.States);
        }

        private sealed class EntryTransition: EntryTransitionBase<ILevelCreationScenarioContext>
        {
            private readonly RemixLevelSocialActionScenarioArgs _args;
            private readonly ILevelHelper _levelHelper;
            private readonly string _naviagationMessage;
            
            public override ScenarioState To { get; }

            public EntryTransition(RemixLevelSocialActionScenarioArgs args, ILevelHelper levelHelper, ScenarioState to,
                string navigationMessage) : base(args)
            {
                _args = args;
                _levelHelper = levelHelper;
                To = to;
                _naviagationMessage = navigationMessage;
            }

            protected override async Task UpdateContext()
            {
                Context.RecommendationId = _args.RecommendationId;
                Context.SocialActionId = _args.ActionId;
                Context.LevelData = _args.Level;
                Context.LevelData.SchoolTaskId = null;
                Context.NavigationMessage = _naviagationMessage;
                var level = _args.Level;
                _levelHelper.InvalidateEventThumbnails(level);
                level.UnlinkTemplates();
                await _levelHelper.PrepareLevelForRemix(level);
                Context.OriginalLevelData = await level.CloneAsync();
                Context.LevelToStartOver = Context.OriginalLevelData;
                
                Context.LevelData = level;
                Context.VideoId = _args.VideoId;
                Context.OnLevelCreationCanceled = _args.OnRemixCanceled;
                Context.PostRecordEditor.OpeningPipState.TargetEventSequenceNumber = 1;
                Context.LevelEditor.OnMoveBack = ScenarioState.PreviousPageExit;
                Context.PostRecordEditor.CheckIfLevelWasModifiedBeforeExit = true;
                
                Context.CharacterSelection.ReasonText = "remix";
                Context.CharacterSelection.PickedCharacters = _args.Characters.ToDictionary(c => c.Id, c => c);
                var originalCharacterIds = Context.LevelData.GetCharacterIds();
                Context.CharacterSelection.CharacterToReplaceIds = originalCharacterIds;
                Context.ExecuteVideoMessageCreationScenario = _args.StartVideoMessageEditorAction;

                ReplaceCharacters(_args.Characters, originalCharacterIds);
                
                await base.UpdateContext();
            }

            private void ReplaceCharacters(
                IReadOnlyList<CharacterFullInfo> newCharacters, IReadOnlyList<long> originalCharacters)
            {
                var charactersReplacementMap = new Dictionary<long, CharacterFullInfo>();
                for (var i = 0; i < newCharacters.Count; i++)
                {
                    charactersReplacementMap.Add(originalCharacters[i], newCharacters[i]);
                }
                
                Context.LevelData.ReplaceCharacters(charactersReplacementMap);
            }
        }
    }
}