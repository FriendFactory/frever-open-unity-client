using System;
using Bridge.Models.ClientServer;
using UIManaging.Common.SelectionPanel;

namespace UIManaging.Pages.UserSelection
{
    public class UserSelectionItemModel: ISelectionItemModel
    {
        public GroupShortInfo ShortProfile { get; }
        public long Id => ShortProfile.Id;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (IsLocked)
                {
                    SelectionChangeLocked?.Invoke();
                    SelectionChanged?.Invoke();
                    return;
                }
                
                if (_isSelected != value)
                {
                    _isSelected = value;
                    SelectionChanged?.Invoke();
                }
            } 
        }

        public bool IsLocked { get; set; }

        public event Action SelectionChanged;
        public event Action SelectionChangeLocked;

        private bool _isSelected;

        public UserSelectionItemModel(GroupShortInfo shortProfile)
        {
            ShortProfile = shortProfile;
        }
    }
}