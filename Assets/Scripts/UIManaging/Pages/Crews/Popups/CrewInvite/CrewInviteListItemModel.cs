using System;
using System.Threading;
using UnityEngine.Serialization;

namespace UIManaging.Pages.Crews.Popups
{
    internal sealed class CrewInviteListItemModel
    {
        public readonly long GroupId;
        public readonly string Username;
        public readonly int FollowersCount;
        public readonly bool Invited;
        public readonly CancellationToken Token;

        public CrewInviteListItemModel(long groupId, string username, int followersCount, bool invited, CancellationToken token)
        {
            GroupId = groupId;
            Username = username;
            FollowersCount = followersCount;
            Invited = invited;
            Token = token;
        }
    }
}