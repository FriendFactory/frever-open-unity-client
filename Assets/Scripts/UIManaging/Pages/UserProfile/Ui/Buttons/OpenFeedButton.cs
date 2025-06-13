using Navigation.Args.Feed;
using Navigation.Core;
using UIManaging.Core;
using UIManaging.Pages.Common.VideoManagement;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.UserProfile.Ui.Buttons
{
    [RequireComponent(typeof(Button))]
    internal sealed class OpenFeedButton : ButtonBase
    {
        [Inject] private PageManager _pageManager;
        [Inject] private VideoManager _videoManager;

        protected override void OnClick()
        {
            if (_pageManager.CurrentPage.Id == PageId.Feed) return;
            if (_pageManager.IsChangingPage) return;
            
            if (ShouldOpenOnLastOpenedState())
            {
                OpenOnLastState();
            }
            else
            {
                _pageManager.MoveNext(PageId.Feed, new GeneralFeedArgs(_videoManager));   
            }
        }

        private bool ShouldOpenOnLastOpenedState()
        {
            var enteredFeedBefore =  _pageManager.HistoryContains(PageId.Feed);
            if (!enteredFeedBefore) return false;

            var wasItGeneralFeed = _pageManager.GetLastArgsForPage(PageId.Feed) is GeneralFeedArgs;
            return wasItGeneralFeed;
        }

        private void OpenOnLastState()
        {
            _pageManager.MoveBackTo(PageId.Feed); 
        }
    }
}