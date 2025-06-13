using System;
using Bridge.Models.ClientServer.Crews;
using UnityEngine;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class SidebarMembersListMemberModel : SidebarMembersListItemModel
    {
        public event Action<SidebarMembersListMemberModel> OnDataChange;

        public readonly long GroupId;
        public readonly long CrewId;
        public readonly long RoleId;
        public readonly bool LocalUserCanEdit;

        public bool IsInitialized { get; private set; }
        public string UserName { get; private set; }
        public Texture2D ProfileImage { get; private set; }
        public bool IsOnline { get; private set; }
        public DateTime LastSeen { get; private set; }
        public string Joined {get; private set; }


        public SidebarMembersListMemberModel(CrewMember crewMember, long crewId, bool localUserCanEdit)
        {
            GroupId = crewMember.Group.Id;
            UserName = crewMember.Group.Nickname;
            IsOnline = crewMember.IsOnline;
            CrewId = crewId;
            RoleId = crewMember.RoleId;
            LastSeen = crewMember.LastLoginTime;
            Joined = crewMember.JoinedCrewTime.ToLocalTime().ToString("yyyy-MM-dd");
            LocalUserCanEdit = localUserCanEdit;
        }

        public void UpdateWithFetchedData(Texture2D portrait)
        {
            IsInitialized = true;
            ProfileImage = portrait;

            OnDataChange?.Invoke(this);
        }
    }
}