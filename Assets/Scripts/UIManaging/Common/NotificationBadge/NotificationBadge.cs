using Common.Abstract;
using Extensions;
using TMPro;
using UnityEngine;

namespace UIManaging.Pages.Common.NotificationBadge
{
    public sealed class NotificationBadge: BaseContextPanel<NotificationBadgeModel>
    {
        [SerializeField] private TMP_Text _count;
        [SerializeField] private bool _showCount = true;

        protected override void OnInitialized()
        {
            ContextData.NotificationCountChanged += UpdateBadge;

            UpdateBadge(ContextData.NotificationCount);
        }

        protected override void BeforeCleanUp()
        {
            ContextData.NotificationCountChanged -= UpdateBadge;
        }

        private void UpdateBadge(int notificationCount)
        {
            if (notificationCount > 0)
            {
                this.SetActive(true);
                _count.text = _showCount ? notificationCount.ToString() : string.Empty;
            }
            else
            {
                this.SetActive(false);
            }
        }
    }
}