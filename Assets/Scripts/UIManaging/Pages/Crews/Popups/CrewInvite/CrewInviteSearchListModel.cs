using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Bridge.Services.UserProfile;
namespace UIManaging.Pages.Crews.Sidebar
{
    public sealed class CrewInviteSearchListModel
    {
        public List<Profile> Friends = new List<Profile>();
        public List<long> InvitedUsers;
        public CancellationToken Token;

        public CrewInviteSearchListModel(IEnumerable<long> invitedUsers, CancellationToken token)
        {
            InvitedUsers = invitedUsers.ToList();
            Token = token;
        }
    }
}