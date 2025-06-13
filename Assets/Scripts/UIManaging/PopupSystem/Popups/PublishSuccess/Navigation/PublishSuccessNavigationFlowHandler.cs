using System;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.VideoServer;
using Common.Publishers;
using Navigation.Args;
using Navigation.Args.Feed;
using Navigation.Core;
using StansAssets.Foundation.Patterns;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.PopupSystem.Configurations;
using UIManaging.PopupSystem.Popups.TaggedUsers;
using UIManaging.SnackBarSystem;
using UnityEngine;
using Zenject;

namespace UIManaging.PopupSystem.Popups.PublishSuccess.Navigation
{
    internal sealed class PublishSuccessNavigationFlowHandler: IInitializable, IDisposable
    {
        private readonly PopupManager _popupManager;
        private readonly PageManager _pageManager;
        private readonly VideoManager _videoManager;
        private readonly IBridge _bridge;
        private readonly SnackBarHelper _snackBarHelper;
        private readonly IPublishVideoHelper _publishVideoHelper;

        public PublishSuccessNavigationFlowHandler(PopupManager popupManager, PageManager pageManager,
            VideoManager videoManager, IBridge bridge, SnackBarHelper snackbarHelper, IPublishVideoHelper publishVideoHelper)
        {
            _popupManager = popupManager;
            _pageManager = pageManager;
            _videoManager = videoManager;
            _bridge = bridge;
            _snackBarHelper = snackbarHelper;
            _publishVideoHelper = publishVideoHelper;
        }

        public void Initialize()
        {
            StaticBus<PublishSuccessNavigationEvent>.Subscribe(OnNavigationEventReceived);
        }
        
        public void Dispose()
        {
            StaticBus<PublishSuccessNavigationEvent>.Unsubscribe(OnNavigationEventReceived);
        }

        private void OnNavigationEventReceived(PublishSuccessNavigationEvent ev) => Navigate(ev.NavigationArgs);

        private async void Navigate(PublishSuccessNavigationArgs args)
        {
            var command = args.NavigationCommand;
            var video = args.Video;
            
            switch (command)
            {
                case PublishSuccessNavigationCommand.Close:
                    Close();
                    break;
                case PublishSuccessNavigationCommand.OriginalCreator:
                    Close();
                    break;
                case PublishSuccessNavigationCommand.VideoInFeed:
                    Close();
                    OpenVideoInFeed(video);
                    break;
                case PublishSuccessNavigationCommand.TaggedUsers:
                    Close();
                    OpenVideoInFeed(video);
                    await WaitUntilNextPageLoaded();
                    OpenTaggedUsersPanel(video);
                    break;
                case PublishSuccessNavigationCommand.Template:
                    Close();
                    OpenVideoInFeed(video);
                    await WaitUntilNextPageLoaded();
                    OpenUsedTemplatePage(video);
                    break;
                case PublishSuccessNavigationCommand.CreatorScore:
                    Close();
                    OpenCreatorScorePage();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(command), command, null);
            }
        }

        private void OpenCreatorScorePage()
        {
            if (_pageManager.IsCurrentPage(PageId.CreatorScore)) return;

            _pageManager.MoveNext(new CreatorScorePageArgs());
        }

        private void OpenTaggedUsersPanel(Video video)
        {
            _popupManager.SetupPopup(new TaggedUsersPopupConfiguration(video.TaggedGroups));
            _popupManager.ShowPopup(PopupType.TaggedUsers);
        }

        private void OpenVideoInFeed(Video video)
        {
            if (_pageManager.IsChangingPage || !CanOpenFeedFromCurrentPage()) return;
            
            var feedArgs = _publishVideoHelper.IsPublishedFromTask
                ? (BaseFeedArgs) new UserTaskFeedArgs(_bridge.Profile.GroupId, _videoManager, video.Id)
                : new LocalUserFeedArgs(_videoManager, video.Id);
            
            _pageManager.MoveNext(PageId.Feed, feedArgs);
        }

        private async void OpenUsedTemplatePage(Video video)
        {
            try
            {
                var response = await _bridge.GetEventTemplate(video.MainTemplate.Id);

                if (response.IsError)
                {
                    _snackBarHelper.ShowInformationSnackBar("Template is unavailable");
                    return;
                }
                
                var pageArgs = new VideosBasedOnTemplatePageArgs
                {
                    TemplateInfo = response.Model,
                    TemplateName =  response.Model.Title,
                    UsageCount =  response.Model.UsageCount
                };
                _pageManager.MoveNext(PageId.VideosBasedOnTemplatePage, pageArgs);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private async Task WaitUntilNextPageLoaded()
        {
            while (_pageManager.IsChangingPage)
            {
                await Task.Delay(42);
            }
        }

        private void Close() => _popupManager.ClosePopupByType(PopupType.PublishSuccess);
        
        private bool CanOpenFeedFromCurrentPage()
        {
            var activePage = _pageManager.CurrentPage.Id;
            return activePage != PageId.LevelEditor && activePage != PageId.PostRecordEditor &&
                   activePage != PageId.UmaEditorNew;
        }
    }
}