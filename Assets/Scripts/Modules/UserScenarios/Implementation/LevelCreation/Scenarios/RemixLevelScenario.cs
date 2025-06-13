using System.Threading.Tasks;
using Bridge.Models.ClientServer.EditorsSetting;
using Bridge.Models.ClientServer.StartPack.Metadata;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
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
    internal sealed class RemixLevelScenario: LevelCreationScenarioBase<RemixLevelScenarioArgs>, IRemixLevelScenario
    {
        [Inject] private LoadingOverlayLocalization _loadingOverlayLocalization;
        [Inject] private FeedLocalization _feedLocalization;
        
        public RemixLevelScenario(DiContainer diContainer, IEditorSettingsProvider editorSettingsProvider, ILevelCreationStatesSetupProvider statesSetupProvider) 
            : base(diContainer, editorSettingsProvider, statesSetupProvider)
        {
        }

        protected override ITransition SetupEntryTransition()
        {
            return new EntryTransition(InitialArgs, Resolve<ILevelHelper>(), ScenarioState.CharacterSelection, _loadingOverlayLocalization.RemixHeader, _feedLocalization, Resolve<IMetadataProvider>());
        }

        protected override Task<IScenarioState[]> SetupStates()
        {
            var setup = StatesSetupProvider.GetStatesSetup(LevelCreationSetup.RemixSetup);
            return Task.FromResult(setup.States);
        }

        protected override Task<EditorsSettings> GetSettings()
        {
            return EditorSettingsProvider.GetRemixEditorSettings();
        }

        private sealed class EntryTransition: EntryTransitionBase<ILevelCreationScenarioContext>
        {
            private readonly RemixLevelScenarioArgs _args;
            private readonly ILevelHelper _levelHelper;
            private readonly string _navigationMessage;

            private readonly FeedLocalization _localization;
            private readonly IMetadataProvider _metadataProvider;

            private MetadataStartPack MetadataStartPack => _metadataProvider.MetadataStartPack;
            
            public override ScenarioState To { get; }

            public EntryTransition(RemixLevelScenarioArgs args, ILevelHelper levelHelper, ScenarioState to,
                string navigationMessage, FeedLocalization localization, IMetadataProvider metadataProvider) : base(args)
            {
                _localization = localization;
                _args = args;
                _levelHelper = levelHelper;
                To = to;
                _navigationMessage = navigationMessage;
                _metadataProvider = metadataProvider;
            }

            protected override async Task UpdateContext()
            {
                Context.LevelData = _args.Level;
                Context.LevelData.SchoolTaskId = null;
                Context.NavigationMessage = _navigationMessage;
                var level = _args.Level;
                _levelHelper.InvalidateEventThumbnails(level);
                Context.LevelData = level;
                Context.VideoId = _args.VideoId;
                Context.InitialTemplateId = _args.InitialTemplateId;
                Context.OnLevelCreationCanceled = _args.OnRemixCanceled;
                Context.PostRecordEditor.OpeningPipState.TargetEventSequenceNumber = 1;
                Context.LevelEditor.OnMoveBack = ScenarioState.PreviousPageExit;
                Context.CharacterSelection.CharacterToReplaceIds = Context.LevelData.GetCharacterIds();
                Context.CharacterSelection.Header = Constants.Features.AI_REMIX_ENABLED 
                    ? _localization.RemixStepTwoHeader 
                    : _localization.RemixVideoHeader;
                Context.CharacterSelection.HeaderDescription = _localization.RemixDescription;
                Context.CharacterSelection.ReasonText = string.Format(_localization.RemixSelectCharactersReasonFormat, Context.CharacterSelection.CharacterToReplaceIds.Length);
                Context.CharacterSelection.Race = MetadataStartPack.GetRaceByUniverseId(_args.Universe.Id); 
                Context.PostRecordEditor.CheckIfLevelWasModifiedBeforeExit = true;
                Context.ExecuteVideoMessageCreationScenario = _args.StartVideoMessageEditorAction;
                await base.UpdateContext();
            }
        }
    }
}