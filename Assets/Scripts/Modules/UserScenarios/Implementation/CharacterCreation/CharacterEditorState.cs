using Extensions;
using JetBrains.Annotations;
using Modules.AssetsStoraging.Core;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using System.Linq;
using Modules.UserScenarios.Implementation.LevelCreation.Helpers;
using UIManaging.Pages.Common;

namespace Modules.UserScenarios.Implementation.CharacterCreation
{
    [UsedImplicitly]
    internal sealed class CharacterEditorState : StateBase<ICharacterCreationContext>, IResolvable
    {
        private readonly PageManagerHelper _pageManagerHelper;
        private readonly IDataFetcher _dataFetcher;
        private readonly AppCacheForceResetHelper _appCacheForceResetHelper;

        public ITransition MoveBack;
        public ITransition MoveNext;
        
        public override ScenarioState Type => ScenarioState.CharacterEditor;
        public override ITransition[] Transitions => new[] { MoveBack, MoveNext }.RemoveNulls();

        public CharacterEditorState(IDataFetcher dataFetcher, PageManagerHelper pageManagerHelper, AppCacheForceResetHelper appCacheForceResetHelper)
        {
            _dataFetcher = dataFetcher;
            _pageManagerHelper = pageManagerHelper;
            _appCacheForceResetHelper = appCacheForceResetHelper;
        }

        public override void Run()
        {
            var args = new UmaEditorArgs
            {
                IsNewCharacter = Context.IsNewCharacter,
                BackButtonAction = OnMoveBack,
                ConfirmButtonAction = OnMoveNext,
                LoadingCancellationRequested = OnCancelledLoading,
                Gender = Context.Gender,
                Style = Context.Style,
                Character = Context.Character,
                CategoryTypeId = Context.CategoryTypeId ?? _dataFetcher.MetadataStartPack.WardrobeCategoryTypes.First().Id,
                ThemeCollectionId = Context.ThemeCollectionId,
                ConfirmActionType = CharacterEditorConfirmActionType.SaveCharacter
            };

            _pageManagerHelper.MoveToUmaEditor(args);
        }

        private async void OnMoveNext()
        {
            await MoveNext.Run();
        }
        
        private async void OnMoveBack()
        {
            await MoveBack.Run();
        }

        private async void OnCancelledLoading()
        {
            _appCacheForceResetHelper.CancelAllLoadings();
            await MoveBack.Run();
            _appCacheForceResetHelper.ClearCache();
        }
    }
}