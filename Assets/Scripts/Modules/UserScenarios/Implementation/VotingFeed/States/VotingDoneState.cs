using System.Collections.Generic;
using Bridge.Models.VideoServer;
using Common.Publishers;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Models;
using Modules.CharacterManagement;
using Modules.UserScenarios.Common;
using Modules.UserScenarios.Implementation.LevelCreation.States;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.PublishPage;
using UIManaging.Pages.VotingFeed;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;

namespace Modules.UserScenarios.Implementation.VotingFeed.States
{
    [UsedImplicitly]
    internal sealed class VotingDoneState : PublishStateBase
    {
        private readonly PopupManager _popupManager;
        private readonly PageManager _pageManager;
        private readonly CharacterManager _characterManager;
        private readonly LocalUserDataHolder _dataHolder;
        
        public override ScenarioState Type => ScenarioState.VotingDone;
        public override ITransition[] Transitions => new[] { MoveNextChat, MoveNextPublic, MoveNextLimitedAccess, MoveBack, PreviewRequested }.RemoveNulls();
        
        public ITransition MoveBack;
        
        public VotingDoneState(PageManager pageManager, CharacterManager characterManager, LocalUserDataHolder dataHolder,
            IPublishVideoPopupManager publishVideoVideoPublishingPopupManager, IPublishVideoController publishVideoController, PopupManager popupManager,
            AmplitudeManager amplitudeManager): 
            base(publishVideoVideoPublishingPopupManager, publishVideoController, amplitudeManager)
        {
            _popupManager = popupManager;
            _pageManager = pageManager;
            _characterManager = characterManager;
            _dataHolder = dataHolder;
        }

        public override void Run()
        {
            var args = new VotingDonePageArgs
            {
                Task = Context.Task,
                DressCodes = new List<string>(),
                CreatedLevel = Context.LevelData,
                UserNickname = _dataHolder.NickName,
                MainCharacterId = _characterManager.SelectedCharacter.Id,
                MainCharacterThumbnail = _characterManager.SelectedCharacter.Files,
                PublishStart = OnPublishRequested,
                MoveBack = OnUserRequestedMovingBack
            };
            
            _pageManager.MoveNext(args, false);
        }

        private void OnPublishRequested()
        {
            var uploadingSettings = new VideoUploadingSettings
            {
                PublishingType = PublishingType.Post,
                PublishInfo = new PublishInfo
                {
                    Access = VideoAccess.Public,
                    ExternalLinkType = ExternalLinkType.Invalid,
                    AllowComment = true,
                }
            };
            
            OnPublishRequested(uploadingSettings);
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