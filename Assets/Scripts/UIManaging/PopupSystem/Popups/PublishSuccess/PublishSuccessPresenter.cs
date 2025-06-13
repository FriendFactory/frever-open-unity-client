using System;
using Bridge;
using Modules.VideoSharing;
using StansAssets.Foundation.Patterns;
using UIManaging.Pages.Common.VideoDetails.VideoAttributes;
using UIManaging.PopupSystem.Popups.PublishSuccess.Navigation;
using UIManaging.PopupSystem.Popups.PublishSuccess.VideoSharing;

namespace UIManaging.PopupSystem.Popups.PublishSuccess
{
    public sealed class PublishSuccessPresenter
    {
        private readonly VideoSharingPresenter _videoSharingPresenter;

        private PublishSuccessView View { get; set; }
        private PublishSuccessModel Model { get; set; } 
        
        public bool IsInitialized { get; private set; }

        public PublishSuccessPresenter(VideoSharingHelper videoSharingHelper, IBridge bridge)
        {
            _videoSharingPresenter = new VideoSharingPresenter(videoSharingHelper, bridge);
        }
        
        public void Initialize(PublishSuccessModel model, PublishSuccessView view)
        {
            Model = model;
            View = view;

            View.CreatorScorePageRequested += OpenCreatorScorePage;
            View.VideoAttributeClicked += DispatchNavigationCommand;
            
            _videoSharingPresenter.Initialize(model.VideoSharingModel, view.VideoSharingPanel);

            _videoSharingPresenter.LoadingToggled += OnVideoSharingLoadingToggled;
            
            IsInitialized = true;
        }

        public void CleanUp()
        {
            View.CreatorScorePageRequested -= OpenCreatorScorePage; 
            View.VideoAttributeClicked -= DispatchNavigationCommand;
            
            _videoSharingPresenter.LoadingToggled -= OnVideoSharingLoadingToggled;
            
            _videoSharingPresenter.CleanUp();
            
            IsInitialized = false;
        }

        private void DispatchNavigationCommand(VideoAttributeType type)
        {
            var navigationCommand = GetVideoAttributeBasedNavigationCommand();
            var navigationArgs = new PublishSuccessNavigationArgs(navigationCommand, Model.Video);
            
            StaticBus<PublishSuccessNavigationEvent>.Post(new PublishSuccessNavigationEvent(navigationArgs));

            PublishSuccessNavigationCommand GetVideoAttributeBasedNavigationCommand()
            {
                switch (type)
                {
                    case VideoAttributeType.OriginalCreator:
                        return PublishSuccessNavigationCommand.OriginalCreator;
                    case VideoAttributeType.TaggedUsers:
                        return PublishSuccessNavigationCommand.TaggedUsers;
                    case VideoAttributeType.UsedTemplate:
                        return PublishSuccessNavigationCommand.Template;
                    case VideoAttributeType.Access:
                    case VideoAttributeType.PartOfTask:
                    case VideoAttributeType.Uploaded:
                    default:
                        throw new ArgumentOutOfRangeException(nameof(type), type, "Navigation is not supported for this video attribute");
                }
            }
        }

        private void DispatchNavigationCommand(PublishSuccessNavigationCommand command)
        {
            var navigationArgs = new PublishSuccessNavigationArgs(command, Model.Video);
            
            StaticBus<PublishSuccessNavigationEvent>.Post(new PublishSuccessNavigationEvent(navigationArgs));
        }

        private void OpenCreatorScorePage() => DispatchNavigationCommand(PublishSuccessNavigationCommand.CreatorScore);
        private void OnVideoSharingLoadingToggled(bool isOn) => View.ToggleLoading(isOn);
    }
}