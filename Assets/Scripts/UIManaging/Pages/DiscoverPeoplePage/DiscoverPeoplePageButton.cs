using System.Linq;
using System.Threading;
using Bridge;
using Common;
using Navigation.Core;
using UIManaging.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.Pages.DiscoverPeoplePage.StarCreator;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.DiscoverPeoplePage
{
    public class DiscoverPeoplePageButton : ButtonBase
    {
        [SerializeField] private GameObject _dotIndicator;

        [Inject] private LocalUserDataHolder _localUserDataHolder;
        [Inject] private IInvitationBridge _bridge;

        private CancellationTokenSource _tokenSource;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        private void Awake()
        {
            _tokenSource = new CancellationTokenSource();
        }

        protected override async void OnEnable()
        {
            base.OnEnable();

            if (_dotIndicator == null)
            {
                return;
            }
            
            var result = await _bridge.GetInvitationCode(_tokenSource.Token);

            if (result.IsRequestCanceled) return;
            if (result.IsError)
            {
                Debug.LogError(result.ErrorMessage);
                return;
            }

            if (_localUserDataHolder.IsStarCreator) return;

            var model = result.Model;
            var hasAvailableRewards = model.InviteGroups != null && model.InviteGroups.Any();
            _dotIndicator.SetActive(PlayerPrefs.GetInt(Constants.DotIndicators.Invite, 0) == 0 || hasAvailableRewards);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _tokenSource?.Cancel();
        }

        private void OnDestroy()
        {
            _tokenSource?.Dispose();
        }

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnClick()
        {
            var isStarCreator = _localUserDataHolder.IsStarCreator;
            var args = isStarCreator
                ? new StarCreatorDiscoverPeoplePageArgs()
                : new DiscoverPeoplePageArgs() as PageArgs;
            var pageId = isStarCreator ? PageId.StarCreatorDiscoverPeople : PageId.DiscoverPeoplePage;
            var transitionArgs = new PageTransitionArgs
            {
                SaveCurrentPageToHistory = true
            };

            Manager.MoveNext(pageId, args, transitionArgs);
        }
    }
}