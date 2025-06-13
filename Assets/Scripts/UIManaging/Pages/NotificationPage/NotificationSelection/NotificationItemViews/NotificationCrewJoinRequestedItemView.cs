using System.Threading.Tasks;
using Common;
using Extensions;
using Modules.Notifications.NotificationItemModels;
using Navigation.Args;
using Navigation.Core;
using TMPro;
using UIManaging.Animated.Behaviours;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationCrewJoinRequestedItemView : UserBasedNotificationItemView<NotificationCrewJoinRequestReceivedItemModel>
    {
        [SerializeField] private Button _respondButton;
        [SerializeField] private TMP_Text _respondStateText;
        [SerializeField] private DisabledButtonShakeBehaviour _disabledButtonShakeBehaviour;
        
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private SnackBarHelper _snackBarHelper;
        
        private bool _isValid;

        protected override string Description => _localization.CrewJoinRequestedFormat;

        protected override void OnInitialized()
        {
            _respondButton.SetActive(false);
            _respondStateText.SetActive(false);
            base.OnInitialized();
        }

        protected override void BeforeCleanup()
        {
            _respondButton.onClick.RemoveAllListeners();
            _disabledButtonShakeBehaviour.OnNonInteractableClick.RemoveAllListeners();
            _isValid = false;
            
            _respondButton.SetActive(false);
            _respondStateText.SetActive(false);
        }

        protected override void OnDescriptionTextClick()
        {
            if (!_isValid) return;
            
            PageManager.MoveNext(PageId.CrewPage, new CrewPageArgs
            {
                OpenJoinRequests = true
            });
        }
        
        protected override async Task LoadContextData()
        {
            await base.LoadContextData();
            CancellationSource.Token.ThrowIfCancellationRequested();
            await _localUser.DownloadProfile();
            CancellationSource.Token.ThrowIfCancellationRequested();
            var crew = await Bridge.GetCrew(ContextData.CrewId, default);
            CancellationSource.Token.ThrowIfCancellationRequested();
            _isValid = _localUser.UserProfile.CrewProfile?.Id == ContextData.CrewId 
                    && crew.IsSuccess && crew.Model.MembersCount > 0;

            var isModerator = _isValid
                           && (_localUser.UserProfile.CrewProfile?.RoleId == Constants.Crew.LEADER_ROLE_ID
                            || _localUser.UserProfile.CrewProfile?.RoleId == Constants.Crew.COORDINATOR_ROLE_ID);
            
            _respondButton.interactable = _isValid && isModerator;
            
            if (_isValid)
            {
                if (isModerator)
                {
                    _respondButton.onClick.AddListener(OnDescriptionTextClick);
                }
                else
                {
                    _disabledButtonShakeBehaviour.OnNonInteractableClick.AddListener(OnModeratorRightsMissingClick);
                }
            }
            else
            {
                _disabledButtonShakeBehaviour.OnNonInteractableClick.AddListener(OnCrewUnavailableClick);
            }

            var requestState = ContextData.RequestId == 0 
                ? 0 
                : PlayerPrefs.GetInt(string.Format(Constants.Crew.JOIN_REQUEST_HANDLED_KEY, ContextData.RequestId), 0);
            var requestHandled = requestState != 0;

            _respondButton.SetActive(!requestHandled);
            _respondStateText.SetActive(requestHandled);
            
            if (requestHandled)
            {
                _respondStateText.text = requestState > 0 
                    ? _localization.CrewRequestAcceptedButton 
                    : _localization.CrewRequestDeniedButton;
            }
        }

        private void OnModeratorRightsMissingClick()
        {
            _snackBarHelper.ShowInformationSnackBar(_localization.NoCrewCoordinatorRightsSnackbarMessage);
        }

        private void OnCrewUnavailableClick()
        {
            _snackBarHelper.ShowInformationSnackBar(_localization.NotACrewMemberSnackbarMessage);
        }
    }
}
