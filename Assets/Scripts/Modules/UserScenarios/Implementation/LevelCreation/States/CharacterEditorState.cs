using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using System.Linq;
using UIManaging.Pages.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.States
{
    [UsedImplicitly]
    internal sealed class CharacterEditorState : StateBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly PageManagerHelper _pageManagerHelper;
        private readonly IDataFetcher _dataFetcher;

        public ITransition MoveBack;
        public ITransition MoveNext;
        
        public override ScenarioState Type => ScenarioState.CharacterEditor;
        public override ITransition[] Transitions => new[] { MoveBack, MoveNext }.RemoveNulls();

        public CharacterEditorState(IDataFetcher dataFetcher, PageManagerHelper pageManagerHelper)
        {
            _dataFetcher = dataFetcher;
            _pageManagerHelper = pageManagerHelper;
        }

        public override void Run()
        {
            var args = new UmaEditorArgs
            {
                BackButtonAction = OnMoveBack,
                ConfirmAction = OnMoveNext,
                Character = Context.CharacterEditor.Character,
                Outfit = Context.CharacterEditor.Outfit,
                CharacterEditorSettings = Context.CharacterEditor.CharacterEditorSettings,
                ConfirmActionType = CharacterEditorConfirmActionType.SaveOutfitAsAutomatic,
                CategoryTypeId = _dataFetcher.MetadataStartPack.WardrobeCategoryTypes.Last().Id,
                ShowTaskInfo = Context.CharacterEditor.ShowTaskInfo,
                TaskFullInfo = Context.Task,
                OutfitsUsedInLevel = Context.LevelData.Event.SelectMany((ev) => ev.CharacterController.Select(controller => controller.OutfitId)).ToHashSet()
            };
            
            Context.CharacterEditor.ShowTaskInfo = false;
           
            _pageManagerHelper.MoveToUmaEditor(args);
        }

        private async void OnMoveNext(CharacterEditorOutput characterEditorOutput)
        {
            Context.CharacterEditor.Outfit = characterEditorOutput.Outfit;
            await MoveNext.Run();
        }
        
        private async void OnMoveBack()
        {
            await MoveBack.Run();
        }
    }
}