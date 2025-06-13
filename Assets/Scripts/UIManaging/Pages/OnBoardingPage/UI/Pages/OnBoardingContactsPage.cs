using Common;
using Modules.AssetsStoraging.Core;
using Navigation.Args.Feed;
using Navigation.Core;
using TMPro;
using UIManaging.Pages.Common.VideoManagement;
using UIManaging.Pages.OnBoardingPage;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.ContactsPage
{
    internal abstract class OnBoardingContactsPage<T> : GenericPage<T> where T : OnBoardingContactsPageArgs
    {
        [SerializeField] private Button _continueButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private TextMeshProUGUI _title;
        
        [Inject] protected PageManager PageManager;
        [Inject] private VideoManager _videoManager;
        [Inject] private IDataFetcher _fetcher;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        private void Awake()
        {
            _continueButton.onClick.AddListener(OnContinueButtonClicked);
            _skipButton.onClick.AddListener(OnSkipButtonClicked);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            _continueButton.onClick.RemoveListener(OnContinueButtonClicked);
            _skipButton.onClick.RemoveListener(OnSkipButtonClicked);
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------
        
        protected override void OnDisplayStart(T args)
        {
            base.OnDisplayStart(args);
            _title.text = args.TitleText;
        }

        protected override void OnInit(PageManager pageManager)
        {
        }
        
        protected abstract void OnContinueButtonClicked();
        
        protected void MoveToFeed()
        {
            ContactsIntroductionComplete(true);
                
            var feedArgs = new GeneralFeedArgs(_videoManager)
            {
                VideoListType = VideoListType.Following
            };
            
            PageManager.MoveNext(PageId.Feed, feedArgs);
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void OnSkipButtonClicked()
        {
            MoveToFeed();
        }

        private void ContactsIntroductionComplete(bool isComplete)
        {
            var isCompleteId = isComplete ? 1 : 0;
            PlayerPrefs.SetInt(Constants.CONTACTS_INTRODUCTION_IDENTIFIER, isCompleteId);
        }
    }
}