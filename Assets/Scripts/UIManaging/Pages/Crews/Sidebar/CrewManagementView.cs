using System.Collections.Generic;
using System.Threading;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Crews;
using Extensions;
using Modules.Crew;
using Navigation.Core;
using UIManaging.Localization;
using UIManaging.Pages.Common.UsersManagement;
using UIManaging.SnackBarSystem;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal class CrewManagementView : BaseContextDataView<CrewManagementViewModel>
    {
        
        [SerializeField] private Toggle _membersTab;
        [SerializeField] private Toggle _motdTab;
        [SerializeField] private Toggle _settingsTab;
        [SerializeField] private Toggle _requestsTab;

        [Space] 
        [SerializeField] private SidebarMemberList _memberList;
        [SerializeField] private SidebarMotdPanel _motdPanel;
        [SerializeField] private SidebarSettingsPanel _settingsPanel;
        [SerializeField] private SidebarRequestPanel _requestPanel;

        [Inject] private IBridge _bridge;
        [Inject] private PageManager _pageManager;
        [Inject] private LocalUserDataHolder _localUser;
        [Inject] private SnackBarHelper _snackBarHelper;
        [Inject] private CrewService _crewService;
        [Inject] private CrewPageLocalization _localization;

        private CancellationTokenSource _tokenSource;
        private SidebarMembersModel _membersModel;
        
        //---------------------------------------------------------------------
        // messages 
        //---------------------------------------------------------------------

        private async void OnEnable()
        {
            _tokenSource = new CancellationTokenSource();
            _membersTab.SetIsOnWithoutNotify(false);
            if (ContextData is null) return;

            await _crewService.RefreshCrewDataAsync(_tokenSource.Token);
            
            _requestsTab.SetActive(!_crewService.Model.IsPublic);
            
            _membersTab.onValueChanged.AddListener(OnMembersToggleValueChange);
            _motdTab.onValueChanged.AddListener(OnMOTDToggleValueChange);
            _settingsTab.onValueChanged.AddListener(OnSettingsToggleValueChange);
            _requestsTab.onValueChanged.AddListener(OnRequestsToggleValueChange);
            
            _crewService.MembersListUpdated += OnMembersListUpdated;
            _crewService.MotDUpdated += OnMotDUpdated;
            if (_crewService.LocalUserIsAdmin)
            {
                _requestPanel.Initialize(ContextData.CrewId);
            }

            if (ContextData.OpenRequestsTab)
            {
                _requestsTab.isOn = true;
            }
            else
            {
                _membersTab.isOn = true;
            }
            _crewService.CrewModelUpdated += OnCrewModelUpdated;
        }

        private void OnDisable()
        {
           _crewService.CrewModelUpdated -= OnCrewModelUpdated;
           _crewService.MembersListUpdated -= OnMembersListUpdated;
           _crewService.MotDUpdated -= OnMotDUpdated;

           _membersTab.interactable = true;
           _membersTab.onValueChanged.RemoveAllListeners();
           _membersTab.SetIsOnWithoutNotify(true);
           _memberList.SetActive(false);
           
           _motdTab.interactable = true;
           _motdTab.onValueChanged.RemoveAllListeners();
           _motdTab.SetIsOnWithoutNotify(false);
           _motdPanel.SetActive(false);
           
           _settingsTab.interactable = true;
           _settingsTab.onValueChanged.RemoveAllListeners();
           _settingsTab.SetIsOnWithoutNotify(false);
           _settingsPanel.SetActive(false);
           
           _requestsTab.interactable = true;
           _requestsTab.onValueChanged.RemoveAllListeners();
           _requestsTab.SetIsOnWithoutNotify(false);
           _requestPanel.SetActive(false);
        }
        
        //---------------------------------------------------------------------
        // BaseContextDataView
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            var role = _crewService.LocalUserMemberData.RoleId;
            var totalCount = _crewService.Model.MembersCount;
            var blockedCount = totalCount - _crewService.Model.Members.Length;
            
            _membersModel =  new SidebarMembersModel(role, ContextData.CrewId, totalCount, blockedCount, ContextData.Members, ContextData.MaxCount, _localization);
            _memberList.Initialize(_membersModel);
            _motdPanel.Initialize(new SidebarMotdPanelModel(role, ContextData.CrewId, ContextData.MessageOfDay));

            _settingsPanel.Initialize(new SidebarSettingsPanelModel(ContextData.ThumbnailOwner, ContextData.CrewId, ContextData.ChatId, ContextData.CrewName,
                                                                    ContextData.CrewDescription,
                                                                    ContextData.Members, ContextData.IsPublic, ContextData.LanguageId));
            if (_crewService.LocalUserIsAdmin)
            {
                _requestPanel.Initialize(ContextData.CrewId);
            }
        }

        //---------------------------------------------------------------------
        // helpers
        //---------------------------------------------------------------------

        private void OnMembersToggleValueChange(bool isOn)
        {
            _membersTab.interactable = !isOn;
            
            _memberList.SetActive(isOn);
        }

        private void OnMOTDToggleValueChange(bool isOn)
        {
            _motdPanel.SetActive(isOn);
            _motdTab.interactable = !isOn;
        }

        private void OnSettingsToggleValueChange(bool isOn)
        {
            _settingsPanel.SetActive(isOn);
            _settingsTab.interactable = !isOn;
        }

        private void OnRequestsToggleValueChange(bool isOn)
        {
            _requestPanel.SetActive(isOn);
            _requestsTab.interactable = !isOn;
        }

        private void OnMotDUpdated(string motd)
        {
            ContextData.MessageOfDay = motd;
        }

        private void OnMembersListUpdated(IReadOnlyCollection<CrewMember> members)
        {
            _membersModel.UpdateMembersList(members);
            _memberList.Refresh();
        }

        private void OnCrewModelUpdated(CrewModel crewModel)
        {
            _requestPanel.Initialize(crewModel.Id);
        }
    }
}