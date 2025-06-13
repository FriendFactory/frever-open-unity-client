using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer.Crews;
using Bridge.Services.UserProfile;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarRequestListModel
    {
        public long CrewId;
        public readonly List<SidebarRequestListItemModel> RequestModels;

        public SidebarRequestListModel(long crewId, CrewMemberRequest[] requests, IReadOnlyCollection<Profile> blockedProfiles)
        {
            CrewId = crewId;
            RequestModels = new List<SidebarRequestListItemModel>();
            for (var i = 0; i < requests.Length; i++)
            {
                var request = requests[i];
                var blocked = blockedProfiles.FirstOrDefault(p => p.MainGroupId == request.Group.Id) != null;
                RequestModels.Add(new SidebarRequestListItemModel(i, request.Group.Id,  request.Id, request.Group.Nickname, blocked));
            }
        }
    }
}