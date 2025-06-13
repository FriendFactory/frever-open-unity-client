using System.Linq;
using Bridge;
using JetBrains.Annotations;
using Modules.AssetsManaging.UncompressedBundles;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation.States;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;

namespace Modules.UserScenarios.Implementation.VotingFeed.States
{
    [UsedImplicitly]
    internal sealed class VotingFeedPostRecordEditorState : PostRecordEditorStateBase
    {
        private readonly PopupManager _popupManager;
       
        public ITransition ExitScenario;
        public override ITransition[] Transitions => base.Transitions.Concat(new [] {ExitScenario}).ToArray();

        protected override bool ShowHints => false;
        
        public VotingFeedPostRecordEditorState(PageManager pageManager, IBridge bridge,
            UncompressedBundlesManager uncompressedBundlesManager, ILevelManager levelManager, PopupManager popupManager,
            PageManagerHelper pageManagerHelper)
            : base(pageManager, bridge, uncompressedBundlesManager, levelManager, pageManagerHelper)
        {
            _popupManager = popupManager;
        }

        protected override void SetupBackRequest(PostRecordEditorArgs postRecordEditorArgs)
        {
            postRecordEditorArgs.CheckIfUserMadeEnoughChangesForTask = false;
            postRecordEditorArgs.OnMovingBackRequested = ShowOptionsPopup;
        }

        private void ShowOptionsPopup(MovingBackData movingBackData)
        {
            var config = new OptionsPopupConfiguration();

            config.AddOption(new Option
            {
                Name = "Exit challenge",
                Color = OptionColor.Red,
                OnSelected = OnExitScenario
            });
            config.AddOption(new Option
            {
                Name = "Go back",
                Color = OptionColor.Default,
                OnSelected = MoveBack
            });
            config.AddOption(new Option
            {
                Name = "Cancel",
                Color = OptionColor.Default,
                OnSelected = ClosePopup
            });
            _popupManager.SetupPopup(config);
            _popupManager.ShowPopup(config.PopupType);
            
            async void OnExitScenario()
            {
                _popupManager.ClosePopupByType(config.PopupType);
                await ExitScenario.Run();
            }
            
            void MoveBack()
            {
                _popupManager.ClosePopupByType(config.PopupType);
                OnMovingBack(movingBackData);
            }
            
            void ClosePopup()
            {
                _popupManager.ClosePopupByType(config.PopupType);
            }
        }
    }
}