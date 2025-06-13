using System;
using Navigation.Args;
using Navigation.Core;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.PublishPage
{
    public class ExternalLinksPage : GenericPage<ExternalLinksPageArgs>
    {
        [SerializeField] private ExternalLinksPanel _externalLinksPanel;
        [SerializeField] private Button _backButton;

        private PageManager _pageManager;
        
        public override PageId Id => PageId.ExternalLinks;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void OnEnable()
        {
            _backButton.onClick.AddListener(OnBackButton);
        }
        
        private void OnDisable()
        {
            _backButton.onClick.RemoveListener(OnBackButton);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnInit(PageManager pageManager)
        {
            _pageManager = pageManager;
        }

        protected override void OnDisplayStart(ExternalLinksPageArgs args)
        {
            base.OnDisplayStart(args);
            
            _externalLinksPanel.Initialize(new ExternalLinksModel
            {
                CurrentLink = args.CurrentLink,
                IsActive = args.IsActive,
                OnSave = args.OnSave
            });
        }

        protected override void OnHidingBegin(Action onComplete)
        {
            _externalLinksPanel.CleanUp();
            
            base.OnHidingBegin(onComplete);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void OnBackButton()
        {
            _pageManager.MoveBack();
        }
    }
}