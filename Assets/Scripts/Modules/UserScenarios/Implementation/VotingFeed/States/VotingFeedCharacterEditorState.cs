using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;

namespace Modules.UserScenarios.Implementation.VotingFeed.States
{
    [UsedImplicitly]
    internal sealed class VotingFeedCharacterEditorState : StateBase<VotingFeedContext>, IResolvable
    {
        private readonly PageManagerHelper _pageManagerHelper;
        private readonly PopupManager _popupManager;

        public ITransition MoveNext;
        public ITransition MoveBack;
        
        public override ScenarioState Type => ScenarioState.VotingFeedCharacterEditor;
        public override ITransition[] Transitions => new[] { MoveNext, MoveBack }.RemoveNulls();

        public VotingFeedCharacterEditorState(PageManagerHelper pageManagerHelper, PopupManager popupManager)
        {
            _pageManagerHelper = pageManagerHelper;
            _popupManager = popupManager;
        }

        public override void Run()
        {
            var args = new UmaEditorArgs
            {
                IsNewCharacter = false,
                BackButtonAction = OnUserRequestedMovingBack,
                ConfirmAction = OnMoveNext,
                Gender = Context.Gender,
                Style = Context.Style,
                Character = Context.CharacterEditor.Character,
                CharacterEditorSettings = Context.CharacterEditor.CharacterEditorSettings,
                Outfit = Context.CharacterEditor.Outfit,
                CategoryTypeId = Constants.Wardrobes.CLOTHES_CATEGORY_TYPE_ID,
                CategoryId = Constants.Wardrobes.OUTFIT_CATEGORY_ID,
                ConfirmActionType = CharacterEditorConfirmActionType.SaveOutfitAsAutomatic,
                ShowHintsOnDisplay = false,
                EnableStoreButton = true
            };
            
            _pageManagerHelper.MoveToUmaEditor(args);
        }

        private async void OnMoveNext(CharacterEditorOutput output)
        {
            Context.CharacterEditor.Outfit = output.Outfit;
            await MoveNext.Run();
        }

        private void OnUserRequestedMovingBack()
        {
            var config = new OptionsPopupConfiguration();
            config.AddOption(new Option
            {
                Name = "Exit challenge",
                Color = OptionColor.Red,
                OnSelected = () =>
                {
                    _popupManager.ClosePopupByType(config.PopupType);
                    OnMoveBack();
                }
            });
            config.AddOption(new Option
            {
                Name = "Cancel",
                Color = OptionColor.Default,
                OnSelected = () => _popupManager.ClosePopupByType(config.PopupType)
            });
            
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
        }

        private async void OnMoveBack()
        {
            await MoveBack.Run();
        }
    }
}