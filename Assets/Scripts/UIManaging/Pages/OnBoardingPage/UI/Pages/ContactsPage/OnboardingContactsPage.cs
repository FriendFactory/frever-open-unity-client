using System;
using System.Linq;
using Bridge;
using Common.Permissions;
using Extensions;
using Modules.Contacts;
using Navigation.Core;
using UIManaging.Common.SelectionPanel;
using UIManaging.PopupSystem;
using UIManaging.PopupSystem.Configurations;
using UnityEngine;
using VoxelBusters.CoreLibrary;
using Zenject;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    public class OnboardingContactsPage: GenericPage<OnboardingContactsPageArgs>
    {
        private const int DEFAULT_SELECTED_ITEMS_COUNT = 20;
        
        [SerializeField] private OnboardingContactsList _contactsList;
        [SerializeField] private OnboardingContactsContinueButton _continueButton;
        [SerializeField] private GameObject _emptyResultsPanel;
        [SerializeField] private GameObject _popupOverlay;
        [SerializeField] private GameObject _description;
        
        [Inject] private PopupManager _popupManager;
        [Inject] private IPermissionsHelper _permissionsHelper;
        [Inject] private IAddressBookContactsProvider _contactsProvider;
        [Inject] private IBridge _bridge;

        private SelectionPanelModel _selectionPanelModel;
        
        public override PageId Id => PageId.OnboardingContactsPage;

        protected override void OnInit(PageManager pageManager)
        {
            _emptyResultsPanel.SetActive(false);
            _popupOverlay.SetActive(true);

            _selectionPanelModel = new SelectionPanelModel(100, Array.Empty<ISelectionItemModel>(), Array.Empty<ISelectionItemModel>());
        }

        protected override void OnDisplayStart(OnboardingContactsPageArgs args)
        {
            base.OnDisplayStart(args);

            var continueButtonArgs = new OnboardingContactsContinueButtonArgs(_selectionPanelModel, OpenPageArgs.OnContinueButtonClick);
            _continueButton.Initialize(continueButtonArgs);
            
            CreatePopup();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void CreatePopup()
        {
            var contactPermissionPopupConfiguration = new DialogPopupConfiguration
            {
                PopupType = PopupType.ContactPermission,
                OnYes = AllowPermission, OnNo = DontAllow
            };

            _popupManager.SetupPopup(contactPermissionPopupConfiguration);
            _popupManager.ShowPopup(contactPermissionPopupConfiguration.PopupType);

            void AllowPermission() => HandleContactsRequest();
            
            void DontAllow()
            {
                _popupManager.ClosePopupByType(PopupType.ContactPermission);
                MoveNext();
            }
        }
        
        private void HandleContactsRequest()
        {
            _popupManager.ClosePopupByType(PopupType.ContactPermission);

            var permissionState = _permissionsHelper.GetPermissionState(PermissionTarget.Contacts);

            switch (permissionState)
            {
                case PermissionStatus.NotDetermined:
                    _permissionsHelper.RequestPermission(PermissionTarget.Contacts, OnContactsPermissionGranted, ContactsPermissionDenied);
                    break;
                case PermissionStatus.Authorized:
                    ReadContacts();
                    break;
                default:
                    MoveNext();
                    break;
            }

            void OnContactsPermissionGranted()
            {
                ReadContacts();
            }

            void ContactsPermissionDenied(string message)
            {
                Debug.Log($"[{GetType().Name}] Permission request failed # {message}");
                MoveNext();
            }
        }

        private void ReadContacts()
        {
            _contactsProvider.ReadContacts(true, OnContactsFetched, OnContactsFetchFailed);

            void OnContactsFetched(ContactsItemModel[] contactItemModels)
            {
                if (IsDestroyed) return;
                
                SetupContactsList(contactItemModels);
            }

            void OnContactsFetchFailed(Error error)
            {
                Debug.LogError($"[{GetType().Name}] Failed to read contacts # {error}");
                MoveNext();
            }
        }

        private void MoveNext()
        {
            _popupOverlay.SetActive(false);
                
            OpenPageArgs.OnContinueButtonClick?.Invoke();
        }

        private async void SetupContactsList(ContactsItemModel[] contactItemModels)
        {
            _popupOverlay.SetActive(false);
                
            if (LinqExtensions.IsNullOrEmpty(contactItemModels))
            {
                _emptyResultsPanel.SetActive(true);
                _description.SetActive(false);
                // show empty results panel
                return;
            }
                
            try
            {
                var contacts = contactItemModels
                              .Where(contact => contact is ContactWithAccountItemModel)
                              .Cast<ContactWithAccountItemModel>();
                
                var listModel = await OnboardingContactsListModel.CreateAsync(_bridge, contacts);

                _selectionPanelModel.AddItems(listModel.Items.ToArray());
                
                listModel.Items.Take(DEFAULT_SELECTED_ITEMS_COUNT).ForEach(item => item.IsSelected = true);
                
                _contactsList.Initialize(listModel);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}