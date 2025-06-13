using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Bridge.Models.ClientServer.Assets;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Models;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class RemixCharacterSelectionToPostRecordEditorTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly AmplitudeManager _amplitudeManager;
        private readonly ILevelHelper _levelHelper;
        
        public override ScenarioState To => ScenarioState.PostRecordEditor;

        public RemixCharacterSelectionToPostRecordEditorTransition(AmplitudeManager amplitudeManager, ILevelHelper levelHelper)
        {
            _amplitudeManager = amplitudeManager;
            _levelHelper = levelHelper;
        }

        protected override async Task UpdateContext()
        {
            SetupCharacter(Context.CharacterSelection.PickedCharacters, Context.LevelData);
            var level = Context.LevelData;
            level.UnlinkTemplates();
            await _levelHelper.PrepareLevelForRemix(level);
            Context.OriginalLevelData = await level.CloneAsync();
            Context.LevelToStartOver = Context.OriginalLevelData;
        }

        protected override Task OnRunning()
        {
            var pickedCharacterIds = Context.CharacterSelection.PickedCharacters.Select(x => x.Value.Id).ToArray();
            StartRemixAmplitudeEvent(Context.LevelData.Id, Context.VideoId.Value, pickedCharacterIds);
            return base.OnRunning();
        }

        private void SetupCharacter(IReadOnlyDictionary<long, CharacterFullInfo> replacingCharactersPairs, Level level)
        {
            level.ReplaceCharacters(replacingCharactersPairs);
            level.RemoveAllOutfits();
        }

        private void StartRemixAmplitudeEvent(long levelId, long videoId, long[] characterIds)
        {
            var startRemixMetaData = new Dictionary<string, object>
            {
                [AmplitudeEventConstants.EventProperties.LEVEL_ID] = levelId,
                [AmplitudeEventConstants.EventProperties.VIDEO_ID] = videoId,
                [AmplitudeEventConstants.EventProperties.CHARACTER_IDS] = characterIds
            };
            _amplitudeManager.LogEventWithEventProperties(AmplitudeEventConstants.EventNames.START_REMIX, startRemixMetaData);
        }
    }
}