using System.Threading;
using System.Threading.Tasks;
using Abstract;
using Bridge;
using Bridge.Models.ClientServer.Crews;
using Bridge.Services.UserProfile;
using Modules.Crew;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarRequestPanel : BaseContextDataView<long>
    {
        [SerializeField] private GameObject _noRequestsPanel;
        [SerializeField] private SidebarRequestList _requestList;

        [Inject] private CrewService _crewService;
        [Inject] private IBridge _bridge;
    
        private CancellationTokenSource _tokenSource;
        
        // ToDo: Move request fetching and all the checks to the crew serivce
        protected override async void OnInitialized()
        {
            if (_crewService.Model.IsPublic || !_crewService.LocalUserIsAdmin) return;
            
            _tokenSource = new CancellationTokenSource();
            _noRequestsPanel.SetActive(false);
            var requests = await FetchJoinRequests();
            var blocked = await FetchBlockedUsers();

            if (requests is null || requests.Length == 0)
            {
                _noRequestsPanel.SetActive(true);
                return;
            }

            var model = new SidebarRequestListModel(ContextData, requests, blocked);
            _requestList.Initialize(model);
        }

        private async Task<CrewMemberRequest[]> FetchJoinRequests()
        {
            var result = await _bridge.GetJoinRequests(ContextData, _tokenSource.Token);
            if (result.IsSuccess) return result.Models;
            
            if (result.IsError) Debug.LogError(result.ErrorMessage);

            return null;
        }

        private async Task<Profile[]> FetchBlockedUsers()
        {
            var result = await _bridge.GetBlockedProfiles(_tokenSource.Token);
            if (result.IsSuccess) return result.Profiles;
            
            if (result.IsError) Debug.LogError(result.ErrorMessage);

            return null;
        }
    }
}