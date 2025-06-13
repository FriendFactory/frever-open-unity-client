using System;
using Bridge.Services.UserProfile;
using UIManaging.Common.SelectionPanel;

namespace UIManaging.Pages.OnBoardingPage.UI.Pages
{
    public class OnboardingContactsItemModel : ISelectionItemModel
    {
        public long Id => Profile.MainGroupId;
        public bool IsLocked { get; set; }
        public string RealName { get; }
        public Profile Profile { get; }

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
                
                if (_isSelected == value) return;

                _isSelected = value;
                SelectionChanged?.Invoke();
            }
        }


        public event Action SelectionChanged;
        public event Action SelectionChangeLocked;

        private bool _isSelected;

        public OnboardingContactsItemModel(string realName, Profile profile)
        {
            RealName = realName;
            Profile = profile;
        }
    }
}