using System.Threading.Tasks;
using Extensions;
using Modules.Notifications.NotificationItemModels;
using Navigation.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationVideoGotRatingItemView : NotificationVideoItemView<NotificationVideoGotRatingItemModel>
    {
        [SerializeField] private Button _overlayButton;
        [Space]
        [SerializeField] private Button _claimButton;
        [SerializeField] private TMP_Text _claimButtonLabel;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        protected override string Description => _localization.YourVideoGotRatingFormat;

        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            base.OnInitialized();
            _overlayButton.onClick.AddListener(GoToVideo);
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _overlayButton.onClick.RemoveListener(GoToVideo);
        }

        protected override async Task LoadContextData()
        {
            await base.LoadContextData();
            UserThumbnail.texture = DefaultUserIcon;

            var isRewardAvailable = Video?.RatingResult?.IsRewardAvailable;
            if (isRewardAvailable != null)
            {
                _claimButton.SetActive(true);
                UpdateClaimButtonState(isRewardAvailable.Value);
            }
            else
            {
                _claimButton.SetActive(false);
            }

        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void GoToVideo()
        {
            PageManager.MoveNext(PageId.Feed, GetVideoArgs());
        }

        private void UpdateClaimButtonState(bool interactable)
        {
            _claimButton.interactable = interactable;
            _claimButtonLabel.text = interactable
                ? _localization.ClaimUserInviteRewardButton
                : _localization.UserInviteRewardClaimedButton;
        }
    }
}