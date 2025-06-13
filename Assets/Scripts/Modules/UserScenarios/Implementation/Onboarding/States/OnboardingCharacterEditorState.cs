using Bridge.Models.ClientServer.Assets;
using Common;
using Extensions;
using Filtering;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;
using TipsManagment;
using UIManaging.Pages.Common;

namespace Modules.UserScenarios.Implementation.Onboarding.States
{
    [UsedImplicitly]
    internal sealed class OnboardingCharacterEditorState : StateBase<OnboardingContext>, IResolvable
    {
        private readonly PageManagerHelper _pageManagerHelper;

        public ITransition MoveNext;
        public ITransition MoveBack;
        
        public override ScenarioState Type => ScenarioState.OnboardingCharacterEditor;
        public override ITransition[] Transitions => new[] { MoveNext, MoveBack }.RemoveNulls();

        public OnboardingCharacterEditorState(PageManagerHelper pageManagerHelper)
        {
            _pageManagerHelper = pageManagerHelper;
        }

        public override void Run()
        {
            var args = new UmaEditorArgs
            {
                IsNewCharacter = Context.IsNewCharacter,
                ConfirmAction = OnMoveNext,
                Gender = Context.Gender,
                Style = Context.Style,
                Character = Context.Character,
                CategoryTypeId = Constants.Wardrobes.BODY_CATEGORY_TYPE_ID,
                DefaultFilteringSetting = new FilteringSetting { AssetPriceFilter = AssetPriceFilter.Free },
                ConfirmActionType = CharacterEditorConfirmActionType.Onboarding,
                ShowHintsOnDisplay = false,
                BackButtonAction = OnMoveBack,
                EnableStoreButton = false
            };
            
            _pageManagerHelper.MoveToUmaEditor(args);
        }

        private async void OnMoveNext(CharacterEditorOutput output)
        {
            Context.CharacterEditor.Character = output.Character;
            Context.CharacterEditor.Outfit = output.Outfit;
            await MoveNext.Run();
        }

        private async void OnMoveBack()
        {
            await MoveBack.Run();
        }
    }
}