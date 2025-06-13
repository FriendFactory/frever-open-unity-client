using System;
using System.Threading;
using Bridge.Models.VideoServer;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.DiscoveryPage
{
    internal sealed class DiscoveryPage : GenericPage<DiscoveryPageArgs>
    {
        public override PageId Id => PageId.DiscoveryPage;

        [SerializeField] private Button _backButton;
        [SerializeField] private DiscoverySearchView _discoverySearchView;
        [Header("Sections")]
        [SerializeField] private CreatorsSection _creatorsSection;
        [SerializeField] private TemplatesSection _templatesSection;
        [SerializeField] private HashtagsSection _hashtagsSection;
        [SerializeField] private PopularVideosSection _videosSection;

        private CancellationTokenSource _cancellationSource;
        private PageManager _pageManager;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _discoverySearchView.HashtagClicked += OnHashtagClicked;
            _hashtagsSection.HashtagClicked += OnHashtagClicked;
        }

        private void OnDisable()
        {
            _discoverySearchView.HashtagClicked -= OnHashtagClicked;
            _hashtagsSection.HashtagClicked -= OnHashtagClicked;
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        protected override void OnDisplayStart(DiscoveryPageArgs args)
        {
            base.OnDisplayStart(args);
            _cancellationSource = new CancellationTokenSource();
            _discoverySearchView.Init(args);
            _backButton.onClick.AddListener(OnBack);

            _creatorsSection.Init();
            _templatesSection.Init();
            _hashtagsSection.Init();
            _videosSection.Init();
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _backButton.onClick.RemoveListener(OnBack);
            _cancellationSource.Cancel();
            _cancellationSource.Dispose();
            base.OnHidingBegin(onComplete);
            _discoverySearchView.Hide();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnBack()
        {
            _pageManager.MoveBack();
        }

        private void OnHashtagClicked(HashtagInfo hashtagInfo)
        {
            var args = new VideosBasedOnHashtagPageArgs()
            {
                TemplateName = hashtagInfo.Name,
                TemplateInfo = null,
                HashtagInfo = hashtagInfo,
                UsageCount = (int)hashtagInfo.UsageCount
            };

            _pageManager.MoveNext(PageId.VideosBasedOnTemplatePage, args);
        }
    }
}