using System.Collections.Generic;
using System.Threading.Tasks;
using Bridge;
using Bridge.Models.ClientServer.Invitation;
using Bridge.Services.UserProfile;
using UnityEngine;

namespace UIManaging.Pages.DiscoverPeoplePage.StarCreator.AcceptedInvitations
{
    internal sealed class AcceptedInvitationsListModel
    {
        private readonly IBridge _bridge;

        private List<Profile> _profiles;

        public IReadOnlyList<Profile> Profiles => _profiles;
        
        public AcceptedInvitationsListModel(IBridge bridge)
        {
            _bridge = bridge;
            _profiles = new List<Profile>();
        }

        public async Task InitializeAsync(IEnumerable<InviteGroup> invitations)
        {
            if (invitations == null) return;

            if (_profiles.Count > 0)
            {
                _profiles.Clear();
            }
            
            foreach (var invitation in invitations)
            {
                var result = await _bridge.GetProfile(invitation.Id);

                if (!result.IsSuccess)
                {
                    Debug.Log($"[{GetType().Name}] Failed to load profile # {result.ErrorMessage}");
                    continue;
                }
                
                _profiles.Add(result.Profile);
            }
        }
    }
}