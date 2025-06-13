using System.Collections.Generic;

namespace UIManaging.Pages.FollowersPage.UI
{
    public class FollowerModel
    {
        public long Id;
        public string Name;
        public List<FollowerModel> Followers;

        public bool IsFollowedBy(long id)
        {
            if (Followers == null) return false;
        
            for (int i = 0; i < Followers.Count; i++)
            {
                if (Followers[i].Id == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}