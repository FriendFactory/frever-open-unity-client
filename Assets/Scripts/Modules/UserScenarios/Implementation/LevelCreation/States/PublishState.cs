using System;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Template;
using Bridge.Models.VideoServer;
using Extensions;
using Modules.Amplitude;
using JetBrains.Annotations;
using Models;
using Modules.AssetsStoraging.Core;
using Modules.LevelManaging.Editing.LevelManagement;
using Modules.UserScenarios.Common;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.PublishPage;
using UnityEngine;

namespace Modules.UserScenarios.Implementation.LevelCreation.States
{
    [UsedImplicitly]
    internal sealed class PublishState: PublishStateBase
    {
        private readonly PageManager _pageManager;
        private readonly ILevelManager _levelManager;
        private readonly IBridge _bridge;
        private readonly IDataFetcher _dataFetcher;
        
        public override ScenarioState Type => ScenarioState.Publish;
        
        public override ITransition[] Transitions => new[] { MoveBack, MoveNextChat, MoveNextPublic, MoveNextLimitedAccess, PreviewRequested, SaveToDraftsRequested }.RemoveNulls();
        
        public ITransition MoveBack;
        public ITransition SaveToDraftsRequested;

        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------

        public PublishState(PageManager pageManager, ILevelManager levelManager, IPublishVideoPopupManager videoPublishingPopupManager, 
            IPublishVideoController publishVideoController, AmplitudeManager amplitudeManager, IBridge bridge, IDataFetcher dataFetcher): base(videoPublishingPopupManager, publishVideoController, amplitudeManager)
        {
            _pageManager = pageManager;
            _levelManager = levelManager;
            _bridge = bridge;
            _dataFetcher = dataFetcher;
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------
        
        public override async void Run()
        {
            try
            {
                Context.PublishContext.VideoPublishSettings.PublishInfo ??= new PublishInfo
                {
                    ExternalLinkType = ExternalLinkType.Invalid
                };
            
                var originalCreator = Context.VideoId.HasValue ? await GetOriginalCreatorAsync(Context.VideoId.Value) : null;
                var originalTemplate = Context.InitialTemplateId.HasValue
                    ? await GetOriginalTemplateAsync(Context.InitialTemplateId.Value)
                    : null;
            
                var pageArgs = new PublishPageArgs
                {
                    LevelData = _levelManager.CurrentLevel,
                    OnPublishRequested = OnPublishRequested,
                    OnMoveBackRequested = OnMoveBackRequested,
                    OnPreviewRequested = OnPreviewRequested,
                    OnSaveToDraftsRequested = OnSaveToDraftsRequested,
                    PublishingType = Context.PublishContext.PublishingType,
                    VideoUploadingSettings = Context.PublishContext.VideoPublishSettings,
                    OriginalCreator = originalCreator,
                    InitialTemplate = originalTemplate,
                };

                if (Context.PublishContext.VideoPublishSettings.MessagePublishInfo != null)
                {
                    pageArgs.ShareDestination = Context.PublishContext.VideoPublishSettings.MessagePublishInfo.ShareDestination;
                }

                if (Context.OpenedFromChat != null)
                {
                    pageArgs.ShareDestination.Chats = new[] { Context.OpenedFromChat.ToChatShortInfo() };
                }
            
                _pageManager.MoveNext(pageArgs);

                base.Run();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private async void OnMoveBackRequested()
        {
            await MoveBack.Run();
            UnsubscribeFromRefocus();
        }
        
        private async void OnSaveToDraftsRequested()
        {
            await SaveToDraftsRequested.Run();
            UnsubscribeFromRefocus();
        }

        private async Task<GroupInfo> GetOriginalCreatorAsync(long videoId)
        {
            try
            {
                var videoResult = await _bridge.GetVideoAsync(videoId);
                if (videoResult.IsError)
                {
                    Debug.LogError($"[{GetType().Name}] Failed to get video # id: {videoId}, reason: {videoResult.ErrorMessage}");
                    return null;
                }

                var video = videoResult.ResultObject;

                return video.Owner;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }

        private async Task<TemplateInfo> GetOriginalTemplateAsync(long originalTemplateId)
        {
            try
            {
                if (_dataFetcher.DefaultTemplateId == originalTemplateId) return null;
                
                var templateResult = await _bridge.GetEventTemplate(originalTemplateId);
                if (templateResult.IsError)
                {
                    Debug.LogError($"[{GetType().Name}] Failed to get template # id: {originalTemplateId}, reason: {templateResult.ErrorMessage}");
                    return null;
                }

                return templateResult.Model;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }
        }
    }
}