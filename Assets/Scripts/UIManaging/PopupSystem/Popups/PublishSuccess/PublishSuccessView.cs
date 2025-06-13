using System;
using Common.Abstract;
using UIManaging.Pages.Common.VideoDetails;
using UIManaging.Pages.Common.VideoDetails.VideoAttributes;
using UIManaging.PopupSystem.Popups.PublishSuccess.Navigation;
using UIManaging.PopupSystem.Popups.PublishSuccess.VideoSharing;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.PublishSuccess
{
    public sealed class PublishSuccessView: BaseContextPanel<PublishSuccessModel>
    {
        [SerializeField] private CreatorScorePanel _creatorScorePanel;
        [SerializeField] private VideoDetailsPanel _videoDetailsPanel;
        [SerializeField] private VideoSharingPanel _videoSharingPanel;
        [SerializeField] private PublishSuccessVideoThumbnailPanel _videoThumbnailPanel;
        [SerializeField] private PublishSuccessNavigationEventDispatcher _navigationEventDispatcher;
        [SerializeField] private CanvasGroup _canvasGroup;
        
        public VideoSharingPanel VideoSharingPanel => _videoSharingPanel;

        public event Action CreatorScorePageRequested;
        public event Action<VideoAttributeType> VideoAttributeClicked;
        
        protected override void OnInitialized()
        {
            _creatorScorePanel.Initialize(ContextData.CreatorScoreModel);
            _videoSharingPanel.Initialize(ContextData.VideoSharingModel);
            _videoDetailsPanel.Initialize(new VideoDetailsModel(ContextData.Video, ContextData.GenerateTemplateWithName));
            _videoThumbnailPanel.Initialize(ContextData.Video);
            _navigationEventDispatcher.Initialize(ContextData.Video);

            _videoDetailsPanel.VideoAttributeClicked += OnVideoAttributeClicked;
            _creatorScorePanel.Selected += OnCreatorScorePageRequested;
        }

        protected override void BeforeCleanUp()
        {
            _creatorScorePanel.CleanUp();
            _videoSharingPanel.CleanUp();
            _videoDetailsPanel.CleanUp();
            _videoThumbnailPanel.CleanUp();
            _navigationEventDispatcher.CleanUp();
            
            _videoDetailsPanel.VideoAttributeClicked -= OnVideoAttributeClicked;
            _creatorScorePanel.Selected -= OnCreatorScorePageRequested;
        }

        private void OnCreatorScorePageRequested() => CreatorScorePageRequested?.Invoke();
        private void OnVideoAttributeClicked(VideoAttributeType type) => VideoAttributeClicked?.Invoke(type);

        public void ToggleLoading(bool isOn)
        {
            _canvasGroup.interactable = !isOn;
        }
    }
}