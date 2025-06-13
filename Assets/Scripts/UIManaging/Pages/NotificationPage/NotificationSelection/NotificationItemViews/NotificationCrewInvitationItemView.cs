using System.Threading.Tasks;
using Bridge.Models.ClientServer.Crews;
using Modules.Notifications.NotificationItemModels;
using Navigation.Args;
using Navigation.Core;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemViews
{
    public class NotificationCrewInvitationItemView : UserBasedNotificationItemView<NotificationCrewInvitationItemModel>
    {
        [SerializeField] private Button _respondButton;
        
        private CrewShortInfo _crewShortInfo;

        [Inject] private LocalUserDataHolder _localUser;

        protected override string Description => _localization.CrewInvitationFormat;

        protected override void BeforeCleanup()
        {
            _respondButton.onClick.RemoveAllListeners();
        }

        protected override void SetupDescriptionText()
        {
            var userName = UserProfile == null ? "Unknown" : UserProfile.NickName;
            _descriptionText.text = string.Format(Description, userName, ContextData.CrewName);
            InvokeDescriptionSet();
        }

        protected override void OnDescriptionTextClick()
        {
            if (_crewShortInfo == null) return;
            PageManager.MoveNext(PageId.CrewInfo, new CrewInfoPageArgs(_crewShortInfo));
        }

        protected override async Task LoadContextData()
        {
            await base.LoadContextData();
            if(ContextData is null || CancellationSource.Token == default) return;
            
            var crewModel = await Bridge.GetCrew(ContextData.CrewId, CancellationSource.Token);
            _respondButton.interactable = crewModel.IsSuccess && _localUser.UserProfile.CrewProfile == null;
            
            if (CancellationSource is null || CancellationSource.IsCancellationRequested) return;
            
            _crewShortInfo = crewModel.Model?.ToCrewShortInfo();
            _respondButton.onClick.AddListener(OnDescriptionTextClick);
        }
    }
}
