using System;
using UIManaging.Pages.Common;
using UnityEngine;

namespace UIManaging.Pages.AppSettingsPage.UI
{
    public class SettingItemViewArgs
    {
        public event Action OnDetailTextChangedEvent;
        
        public string Header;
        public string Title;
        public Color TitleColor = Color.black;
        public Sprite Icon;
        public Sprite CustomArrowIcon;

        public string Description;
        public string DetailText;

        public bool ShowTopDivider;
        public bool ShowBottomDivider;
        public bool ShowArrow;
        public bool ShowToggle;
        public bool IsToggleOn;

        public Action OnClicked;
        public Action<bool> OnToggleValueChanged;

        public NotificationBadgeModel NotificationBadgeModel;

        public bool ShowDescription => !string.IsNullOrWhiteSpace(Description);
        public bool ShowTitle => !string.IsNullOrWhiteSpace(Title);
        public bool ShowDetailText => !string.IsNullOrWhiteSpace(DetailText);
        public bool ShowHeader => !string.IsNullOrWhiteSpace(Header);
        public bool ShowCustomArrowIcon => CustomArrowIcon != null;
        public bool ShowDetailPanel => ShowToggle || ShowArrow || ShowDescription || ShowTopDivider || ShowBottomDivider;

        public void SetDetailText(string detailText)
        {
            DetailText = detailText;
            OnDetailTextChangedEvent?.Invoke();
        }
    }
}