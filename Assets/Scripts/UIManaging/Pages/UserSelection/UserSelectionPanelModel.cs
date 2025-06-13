using System.Collections.Generic;
using System.Linq;
using Bridge.Models.ClientServer;
using Navigation.Args;
using UIManaging.Common.SearchPanel;
using UIManaging.Common.SelectionPanel;

namespace UIManaging.Pages.UserSelection
{
    public class UserSelectionPanelModel: SelectionPanelModel<UserSelectionItemModel>
    {
        public IEnumerable<long> FilterGroupIds { get; }
        public long? TargetProfileId { get;  }
        public UsersFilter Filter { get; }
        
        public UserSelectionPanelModel(int maxSelected, ICollection<UserSelectionItemModel> preselectedModels,
            ICollection<UserSelectionItemModel> lockedModels, long? targetProfileId, UserSelectionPageArgs.UsersFilter filter,
            IEnumerable<long> filterGroupIds = null)
            : base(maxSelected, preselectedModels, lockedModels)
        {
            FilterGroupIds = filterGroupIds;
            TargetProfileId = targetProfileId;
            
            switch (filter)
            {
                case UserSelectionPageArgs.UsersFilter.All:
                    Filter = UsersFilter.All;
                    break;
                case UserSelectionPageArgs.UsersFilter.Friends:
                    Filter = UsersFilter.Friends;
                    break;
                case UserSelectionPageArgs.UsersFilter.Followers:
                    Filter = UsersFilter.Followers;
                    break;
                case UserSelectionPageArgs.UsersFilter.Followed:
                    Filter = UsersFilter.Followed;
                    break;
            }
        }

        public UserSelectionPanelModel(int maxSelected, long? targetProfileId, UserSelectionPageArgs.UsersFilter filter) : 
            this(maxSelected, new List<UserSelectionItemModel>(), new List<UserSelectionItemModel>(), targetProfileId, filter)
        {
            
        }

        public virtual void AddItems(ICollection<GroupShortInfo> profiles)
        {
            AddItems(profiles.Select(profile => new UserSelectionItemModel(profile)).ToList());
        }
    }
}