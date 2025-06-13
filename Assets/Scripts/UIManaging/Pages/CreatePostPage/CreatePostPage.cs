using System;
using System.Threading.Tasks;
using Common;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.CreatePostPage;
using UIManaging.Pages.CreatePostPage.TemplateGallery;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Tasks
{
    public sealed class CreatePostPage : GenericPage<CreatePostPageArgs>
    {
        [SerializeField] private TemplateGalleryPanel _templateGalleryPanel;
        [SerializeField] private CreateButtonWidget _createButtonWidget;

        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private PopupManager _popupManager;
        
        public override PageId Id => PageId.CreatePost;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInit(PageManager pageManager)
        {
            _templateGalleryPanel.OnGridScrolled += OnGridScrolled;
        }

        protected override void OnDisplayStart(CreatePostPageArgs args)
        {
            base.OnDisplayStart(args);
            CheckVideoToFeedUnlocked();
            
            _templateGalleryPanel.OnGridLoaded += SubscribeScrollDelayed;

            if (args.Backed && _templateGalleryPanel.FirstPageLoaded)
            {
                _templateGalleryPanel.RefreshTabs();
                return;
            }
            
            _templateGalleryPanel.Init(OpenPageArgs.GalleryState as TemplateGalleryState);
        }

        private async void CheckVideoToFeedUnlocked()
        {
            await _localUser.DownloadProfile();
            if (PlayerPrefs.HasKey(Constants.Features.VIDEO_TO_FEED_UNLOCKED_POPUP_DISPLAYED) 
                || !_localUser.LevelingProgress.AllowVideoToFeed)
            {
                return;
            }
            PlayerPrefs.SetInt(Constants.Features.VIDEO_TO_FEED_UNLOCKED_POPUP_DISPLAYED, 1);
            _popupManager.PushPopupToQueue(new AlertPopupConfiguration {PopupType = PopupType.VideoToFeedUnlocked});
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            base.OnHidingBegin(onComplete);
            _templateGalleryPanel.OnGridScrolled -= OnGridScrolled;
            _createButtonWidget.PlayAnimation(false, true);
        }
        
        private void OnGridScrolled()
        {
            _templateGalleryPanel.OnGridScrolled -= OnGridScrolled;
            _createButtonWidget.PlayAnimation(false);
        }

        private async void SubscribeScrollDelayed()
        {
            _templateGalleryPanel.OnGridLoaded -= SubscribeScrollDelayed;
            await Task.Delay(500);
            if (IsDestroyed) return;
            _createButtonWidget.PlayAnimation(true);
            _templateGalleryPanel.OnGridScrolled += OnGridScrolled;
        }
    }
}