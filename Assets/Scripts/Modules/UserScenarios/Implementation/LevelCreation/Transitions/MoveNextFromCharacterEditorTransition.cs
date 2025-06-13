using System;
using System.Threading.Tasks;
using Extensions;
using JetBrains.Annotations;
using Models;
using Modules.UserScenarios.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.Transitions
{
    [UsedImplicitly]
    internal sealed class MoveNextFromCharacterEditorTransition: TransitionBase<ILevelCreationScenarioContext>, IResolvable
    {
        public override ScenarioState To => Context.CharacterEditor.OpenedFrom;

        protected override Task UpdateContext()
        {
            if (To == ScenarioState.LevelEditor) Context.LevelEditor.ShowLoadingPagePopup = true;
            if (To == ScenarioState.PostRecordEditor) Context.PostRecordEditor.ShowPageLoadingPopup = true;
            
            return Task.CompletedTask;
        }

        protected override Task OnRunning()
        {
            ApplyNewCreatedOutfit();
            
            if (To == ScenarioState.PostRecordEditor)
            {
                GetTargetEvent().HasActualThumbnail = false;
            }
            
            return Task.CompletedTask;
        }

        private void ApplyNewCreatedOutfit()
        {
            var outfit = Context.CharacterEditor.Outfit;
            var ev = GetTargetEvent();
            var targetCharacterId = Context.CharacterEditor.Character.Id;
            var characterController = ev.GetCharacterControllerByCharacterId(targetCharacterId);
            characterController.SetOutfit(outfit);
        }

        private Event GetTargetEvent()
        {
            switch (To)
            {
                case ScenarioState.LevelEditor:
                    return Context.LevelEditor.DraftEventData.Event;
                case ScenarioState.PostRecordEditor:
                    var targetEventIndex = Context.PostRecordEditor.OpeningPipState.TargetEventSequenceNumber;
                    return Context.LevelData.GetEventBySequenceNumber(targetEventIndex);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}