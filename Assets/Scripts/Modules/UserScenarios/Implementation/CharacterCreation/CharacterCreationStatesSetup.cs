using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.Common;
using Zenject;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class CharacterCreationStatesSetup : StatesSetupBase, IResolvable
    {
        public CharacterCreationStatesSetup(DiContainer diContainer) : base(diContainer)
        {
        }

        protected override IScenarioState[] SetupStates()
        {
            return GetCombinedFlow();
        }

        private IScenarioState[] GetCombinedFlow()
        {
            var raceSelection = ResolveState<RaceSelectionState>();
            var combinedStyleSelection = ResolveState<CharacterStyleSelectionState>();
            var characterEditor = ResolveState<CharacterEditorState>();
            var selfieTaking = ResolveState<TakingSelfieState>();
            var avatarPreview = ResolveState<AvatarPreviewState>();
            var exit = ResolveState<PreviousPageExitState>();

            raceSelection.MoveBack = new EmptyTransition(exit);
            raceSelection.MoveNext = new EmptyTransition(combinedStyleSelection);
            
            combinedStyleSelection.MoveBack = ResolveTransition<CharacterStyleSelectionMoveBackTransition>();
            combinedStyleSelection.SelfieSelectedTransition = new EmptyTransition(selfieTaking);
            combinedStyleSelection.StyleSelectedTransition = new EmptyTransition(characterEditor);

            characterEditor.MoveBack = ResolveTransition<BackFromCharacterEditorTransition>();
            characterEditor.MoveNext = new EmptyTransition(exit);
            
            selfieTaking.MoveBack = new EmptyTransition(combinedStyleSelection);
            selfieTaking.OnSelfieCapturedTransition = new EmptyTransition(avatarPreview);

            avatarPreview.MoveBack = ResolveTransition<AvatarPreviewToSelfieTakingTransition>();
            avatarPreview.MoveNext = ResolveTransition<AvatarPreviewToCharacterEditorTransition>();
            
            return new IScenarioState[] { combinedStyleSelection, raceSelection, characterEditor, selfieTaking, avatarPreview, exit };
        }
    }
}