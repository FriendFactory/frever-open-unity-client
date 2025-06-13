using System;

namespace UIManaging.Pages.Crews.Sidebar
{
    internal sealed class ViewAllCrewMembersListItemModel
    {
        private readonly long _localUserId;

        public bool Initialized { get; private set; }
        
        public bool IsLocalUser => GroupId == _localUserId;
        public readonly string Place;
        public long GroupId { get; private set; }
        public bool IsOnline { get; private set; }
        public string Username { get; private set; }
        public string Score { get; private set; }

        public event Action DataFetched;

        public ViewAllCrewMembersListItemModel(int place, long localUserId)
        {
            _localUserId = localUserId;
            Place = place.ToString();
        }

        public void Update(long groupId, bool isOnline, string username, string score)
        {
            Initialized = true;

            GroupId = groupId;
            IsOnline = isOnline;
            Username = username;
            Score = score;
            
            DataFetched?.Invoke();
        }
    }
}