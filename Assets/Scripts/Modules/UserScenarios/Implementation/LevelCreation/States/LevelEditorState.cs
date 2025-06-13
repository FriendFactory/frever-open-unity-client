using System.Linq;
using Bridge.Models.ClientServer.Template;
using Common;
using Extensions;
using JetBrains.Annotations;
using Modules.LevelManaging.Editing.Templates;
using Modules.TempSaves.Manager;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation.Helpers;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common;

namespace Modules.UserScenarios.Implementation.LevelCreation.States
{
    [UsedImplicitly]
    internal sealed class LevelEditorState: StateBase<ILevelCreationScenarioContext>, IResolvable
    {
        private readonly PageManagerHelper _pageManagerHelper;
        private readonly PageManager _pageManager;
        private readonly TempFileManager _tempFileManager;
        private readonly AppCacheForceResetHelper _appCacheForceResetHelper;

        public ITransition MoveNext;
        public ITransition MoveBack;
        public ITransition DeployFromGallery;
        public ITransition OutfitCreationTransition;
        public ITransition CreateVideoMessageTransition;

        public override ScenarioState Type => ScenarioState.LevelEditor;
        public override ITransition[] Transitions => new[] { MoveNext, MoveBack, DeployFromGallery, OutfitCreationTransition, CreateVideoMessageTransition }.RemoveNulls();

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public LevelEditorState(PageManager pageManager, TempFileManager tempFileManager, PageManagerHelper pageManagerHelper, AppCacheForceResetHelper appCacheForceResetHelper)
        {
            _pageManagerHelper = pageManagerHelper;
            _appCacheForceResetHelper = appCacheForceResetHelper;
            _pageManager = pageManager;
            _tempFileManager = tempFileManager;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public override void Run()
        {
            var pageArgs = new LevelEditorArgs
            {
                LevelData = Context.LevelData,
                OriginalLevelData = Context.OriginalLevelData,
                LevelToStartOver = Context.LevelToStartOver,
                DraftEventData = Context.LevelEditor.DraftEventData,
                LinkTemplateToEvent = Context.LevelEditor.LinkTemplateToEvent,
                TaskFullInfo = Context.Task,
                ExitButtonBehaviour = Context.LevelEditor.ExitButtonBehaviour,
                TemplateHashtagInfo = Context.Hashtag,
                OpenVideoUploadMenu = Context.LevelEditor.OpenVideoUploadMenu,
                ReplaceCharactersData = Context.LevelEditor.CharactersToUseInTemplate?
                   .Select(x=> new ReplaceCharacterData(x.Key, x.Value)).ToArray(),
                Settings = Context.LevelEditor.LevelEditorSettings,
                NavigationMessage = Context.NavigationMessage,
                OnLevelEditorLoaded = Context.LevelEditor.OnLevelEditorLoaded,
                NewEventsDeletionOnly = Context.LevelEditor.NewEventsDeletionOnly,
                ShowHintsOnDisplay = false,
                ShowTemplateCreationStep = Context.LevelEditor.ShowTemplateCreationStep && (Context.LevelData == null || Context.LevelData.IsEmpty()),
                ShowDressingStep = Context.LevelEditor.ShowDressingStep && (Context.LevelData == null || Context.LevelData.IsEmpty()),
                ShowTaskInfo = Context.LevelEditor.ShowTaskInfo,
                Music = Context.Music
            };

            Context.LevelEditor.ShowTaskInfo = false;
            
            if (Context.LevelEditor.TemplateId.HasValue)
            {
                pageArgs.Template = new TemplateInfo
                {
                    Id = Context.LevelEditor.TemplateId.Value
                };
            }

            pageArgs.CancelLoadingAction = OnCancelLoadingRequested;

            pageArgs.OnMovingForwardRequested += x =>
            {
                Context.NavigationMessage = x.NavigationMessage;
                MoveNext.Run();
            };

            pageArgs.OnExitRequested = OnExitRequested;

            pageArgs.OnStartOverRequested = () =>
            {
                Context.LevelData = Context.LevelToStartOver.Clone();
                _tempFileManager.SaveDataLocally(Context.LevelToStartOver, Constants.FileDefaultPaths.LEVEL_TEMP_PATH);
                Context.LevelEditor.TemplateId = Context.InitialTemplateId;
                Context.LevelEditor.DraftEventData = default;
                Context.LevelEditor.CharactersToUseInTemplate = Context.CharacterSelection.PickedCharacters;
                _pageManager.CurrentPage.Hide();
                Run();
            };

            pageArgs.OnOutfitCreationRequested = OnOutfitCreationRequested;
            pageArgs.OnCreateVideoMessageRequested = OnEnterVideoMessageCreationRequested;
            
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true,
            };
            
            pageArgs.OnOpenVideoUploadPageRequested += OnVideoUploadPageRequested;

            if (Context.LevelEditor.ShowLoadingPagePopup)
            {
                _pageManagerHelper.MoveToLevelEditor(pageArgs, true);
                return;
            }
            
            _pageManager.MoveNext(pageArgs, transitionArgs);
        }

        private async void OnVideoUploadPageRequested(NonLeveVideoData videoData)
        {
            Context.PublishContext.NonLevelVideoData = videoData;
            await DeployFromGallery.Run();
        }
        
        private async void OnOutfitCreationRequested(LevelEditorCreationOutfitReqData model)
        {
            Context.CharacterEditor.OpenedFrom = Type;
            Context.CharacterEditor.Character = model.TargetCharacter;
            Context.CharacterEditor.Outfit = model.CurrentOutfit;
            Context.LevelEditor.DraftEventData = model.DraftEventData;
            Context.LevelData = model.Level;
            await OutfitCreationTransition.Run();
        }

        private void OnCancelLoadingRequested()
        {
            OnExitRequested(new LevelEditorExitArgs()
            {
                SavedToDraft = Context.SavedAsDraft
            });

            _appCacheForceResetHelper.CancelAllLoadings();
            _appCacheForceResetHelper.ClearCache();
        }

        private void OnExitRequested(LevelEditorExitArgs exitArgs)
        {
            Context.SavedAsDraft = exitArgs.SavedToDraft;
            MoveBack.Run();
        }

        private void OnEnterVideoMessageCreationRequested()
        {
            CreateVideoMessageTransition.Run();
        }
    }
}