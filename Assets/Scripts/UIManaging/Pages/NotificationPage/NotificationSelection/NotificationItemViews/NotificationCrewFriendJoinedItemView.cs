using System.Threading.Tasks;
using Bridge;
using Modules.Notifications.NotificationItemModels;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using Zenject;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationCrewFriendJoinedItemView : UserBasedNotificationItemView<NotificationCrewFriendJoinedModel>
    {
        protected override string Description => _localization.CrewFriendJoinedFormat;

        [Inject] private IBridge _bridge;
        [Inject] private LocalUserDataHolder _localUser;

        private bool _isValid;
        
        protected override void OnDescriptionTextClick()
        {
            if (!_isValid) return;
            PageManager.MoveNext(PageId.CrewPage, new CrewPageArgs());
        }

        protected override async Task LoadContextData()
        {
            await base.LoadContextData();
            CancellationSource.Token.ThrowIfCancellationRequested();
            await _localUser.DownloadProfile();
            CancellationSource.Token.ThrowIfCancellationRequested();
            var crew = await _bridge.GetCrew(ContextData.CrewId, default);
            CancellationSource.Token.ThrowIfCancellationRequested();
            _isValid = _localUser.UserProfile.CrewProfile?.Id == ContextData.CrewId 
                && crew.IsSuccess && crew.Model.MembersCount > 0;
        }
    }
}
