using Common.Permissions;
using Modules.Amplitude;
using Modules.Contacts;
using Navigation.Core;
using UIManaging.Common.Args.Buttons;
using UIManaging.Common.PageHeader;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using Zenject;

namespace UIManaging.Pages.ContactsPage
{
    public sealed class ContactsPage : GenericPage<ContactsPageArgs>
    {
        [SerializeField] private PageHeaderView _pageHeaderView;
        [SerializeField] private ContactsListView _contactsListView;
        [SerializeField] private GameObject _noContactsText;
        [SerializeField] private GameObject _permissionDeniedMessagePanel;
        
        [Inject] private PageManager _pageManager;
        [Inject] private AmplitudeManager _amplitudeManager;
        [Inject] private IAddressBookContactsProvider _contactsProvider;
        [Inject] private PopupManager _popupManager;
        [Inject] private IPermissionsHelper _permissionsHelper;

        private ContactsPageLoc _loc;

        //---------------------------------------------------------------------
        // Properties
        //---------------------------------------------------------------------

        public override PageId Id => PageId.ContactsPage;
        
        protected override void OnInit(PageManager pageManager)
        {
            _loc = GetComponent<ContactsPageLoc>();
            _pageHeaderView.Init(new PageHeaderArgs(_loc.PageHeader, new ButtonArgs(string.Empty, _pageManager.MoveBack)));
        }

        protected override void OnDisplayStart(ContactsPageArgs args)
        {
            base.OnDisplayStart(args);
            
            _noContactsText.SetActive(false);
            _permissionDeniedMessagePanel.SetActive(false);
            
            RequestContacts();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void RequestContacts()
        {
            var permissionStatus = _permissionsHelper.GetPermissionState(PermissionTarget.Contacts);
            switch (permissionStatus)
            {
                case PermissionStatus.Authorized:
                    ReadContacts();
                    break;
                case PermissionStatus.NotDetermined:
                    CreatePopup();
                    break;
                default:
                    _permissionDeniedMessagePanel.SetActive(true);
                    break;
            }
        }

        private void CreatePopup()
        {
            var contactPermissionPopupConfiguration = new DialogPopupConfiguration
            {
                PopupType = PopupType.ContactPermission,
                Title = _loc.FindPopupTitle,
                Description = _loc.FindPopupDesc,
                YesButtonText = _loc.FindPopupConfirm,
                NoButtonText = _loc.FindPopupCancel,
                OnYes = AllowPermission, OnNo = DontAllow
            };

            _popupManager.SetupPopup(contactPermissionPopupConfiguration);
            _popupManager.ShowPopup(contactPermissionPopupConfiguration.PopupType);

            void AllowPermission() => HandleContactsRequest();

            void DontAllow()
            {
                _popupManager.ClosePopupByType(PopupType.ContactPermission);
                _permissionDeniedMessagePanel.SetActive(true);
            }
        }

        private void HandleContactsRequest()
        {
            _popupManager.ClosePopupByType(PopupType.ContactPermission);

            _permissionsHelper.RequestPermission(PermissionTarget.Contacts, OnContactsAccessGranted, OnContactsAccessDenied);

            void OnContactsAccessGranted()
            {
                if (IsDestroyed) return;
                
                ReadContacts();
            }

            void OnContactsAccessDenied(string _)
            {
                _permissionDeniedMessagePanel.SetActive(true);
            }
        }

        private void ReadContacts()
        {
            _contactsProvider.ReadContacts(false, OnContactsFetched, OnContactsFetchFailed);
                
            void OnContactsFetched(ContactsItemModel[] contactItemModels)
            {
                if (IsDestroyed) return;

                SetupContactsList(contactItemModels);
            }
            
            void OnContactsFetchFailed(Error error)
            {
                Debug.LogError($"[{GetType().Name}] Failed to read contacts # {error}");
                _noContactsText.SetActive(true);
            }
        }

        private void SetupContactsList(ContactsItemModel[] contactItemModels)
        {
            var contactsAreAvailable = contactItemModels?.Length > 0;
            _noContactsText.SetActive(!contactsAreAvailable);
            if (!contactsAreAvailable) return;
            
            var contactsListModel = new ContactsListModel(contactItemModels);
            _contactsListView.Initialize(contactsListModel);
            _amplitudeManager.LogEvent(AmplitudeEventConstants.EventNames.ADD_FRIENDS);
        }
    }
}