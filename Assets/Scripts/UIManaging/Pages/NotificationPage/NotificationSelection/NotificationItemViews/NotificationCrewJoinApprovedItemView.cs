using System.Threading.Tasks;
using Modules.Notifications.NotificationItemModels;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationCrewJoinApprovedItemView : UserBasedNotificationItemView<NotificationCrewJoinRequestAcceptedItemModel>
    {
        private bool _isValid;

        [Inject] private LocalUserDataHolder _localUser;

        protected override string Description => _localization.CrewJoinApprovedFormat;
        
        protected override void OnDescriptionTextClick()
        {
            if (!_isValid) return;
            PageManager.MoveNext(PageId.CrewPage, new CrewPageArgs());
        }
        
        protected override void SetupDescriptionText()
        {
            _descriptionTextButton.onClick.AddListener(OnDescriptionTextClick);
            _descriptionText.text = string.Format(Description, ContextData.CrewName);
            InvokeDescriptionSet();
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();
            _isValid = false;
        }

        protected override async Task LoadContextData()
        {
            await base.LoadContextData();
            await _localUser.DownloadProfile();
            if (ContextData == null
             || CancellationSource.Token == default
             || _localUser.UserProfile.CrewProfile?.Id != ContextData.CrewId)
            {
                return;
            }
            
            var crewModel = await Bridge.GetCrew(ContextData.CrewId, CancellationSource.Token);

            if (CancellationSource is null || CancellationSource.IsCancellationRequested) return;

            _isValid = crewModel.IsSuccess && crewModel.Model?.MembersCount > 0;
        }
    }
}
