using Bridge.Models.ClientServer.StartPack.Metadata;
using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using CharacterInfo = Bridge.Models.ClientServer.Assets.CharacterInfo;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class CharacterStyleSelectionState : StateBase<ICharacterCreationContext>, IResolvable
    {
        private readonly PageManager _pageManager;
        
        public ITransition StyleSelectedTransition;
        public ITransition SelfieSelectedTransition;
        public ITransition MoveBack;

        public override ScenarioState Type => ScenarioState.CombinedStyleSelection;
        public override ITransition[] Transitions => new[] { MoveBack, StyleSelectedTransition, SelfieSelectedTransition }.RemoveNulls();

        public CharacterStyleSelectionState(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        public override void Run()
        {
            var args = new CharacterStyleSelectionArgs
            {
                SelectedStyle = Context.Style?.Id,
                OnBackButtonClicked = OnMoveBack,
                OnStyleSelected = OnMoveNext,
                OnSelfieButtonClicked = OnSelfieSelected,
                SelectedGender = Context.Gender,
                SelectedCreateMode = Context.SelectedCreateMode,
                Race = Context.Race,
                OnPageDispayed = Context.OnDisplayed,
            };
            _pageManager.MoveNext(args);
        }

        private async void OnMoveBack(CharacterInfo lastSeenStyle, Gender gender)
        {
            Context.Style = lastSeenStyle;
            Context.Gender = gender;
            Context.SelectedCreateMode = CreateMode.Preset;
            await MoveBack.Run();
        }

        private async void OnMoveNext(CharacterInfo style, Gender gender)
        {
            Context.Style = style;
            Context.Gender = gender;
            Context.SelectedCreateMode = CreateMode.Preset;
            await StyleSelectedTransition.Run();
        }

        private async void OnSelfieSelected(Gender gender)
        {
            Context.SelectedCreateMode = CreateMode.Selfie;
            Context.Gender = gender;
            await SelfieSelectedTransition.Run();
        }
    }
}