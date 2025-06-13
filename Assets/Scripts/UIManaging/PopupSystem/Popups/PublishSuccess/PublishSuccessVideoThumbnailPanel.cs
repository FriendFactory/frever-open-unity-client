using Bridge.Models.VideoServer;
using Common.Abstract;
using Navigation.Args;
using StansAssets.Foundation.Patterns;
using UIManaging.Common;
using UIManaging.PopupSystem.Popups.PublishSuccess.Navigation;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.PublishSuccess
{
    internal sealed class PublishSuccessVideoThumbnailPanel: BaseContextSelectablePanel<Video>
    {
        [SerializeField] private VideoThumbnail _videoThumbnail;

        protected override void OnInitialized()
        {
            base.OnInitialized();
            
            _videoThumbnail.Initialize(new VideoThumbnailModel(ContextData.ThumbnailUrl));
        }

        protected override void BeforeCleanUp()
        {
            base.BeforeCleanUp();
            
            _videoThumbnail.CleanUp();
        }

        protected override void OnSelected()
        {
            base.OnSelected();
            
            var args = new PublishSuccessNavigationArgs(PublishSuccessNavigationCommand.VideoInFeed, ContextData);
            
            StaticBus<PublishSuccessNavigationEvent>.Post(new PublishSuccessNavigationEvent(args));
        }
    }
}