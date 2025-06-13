using Common.Abstract;
using UIManaging.Pages.Feed.GamifiedFeed;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.Feed.Core
{
    internal sealed class CommentsButton: BaseContextPanel<CommentsButtonModel>
    {
        [SerializeField] private Button _button;
        [SerializeField] private VideoKPICount _count;
        [SerializeField] private GameObject _notificationBadgeIcon;

        protected override bool IsReinitializable => true; 

        public void RefreshCount(long count)
        {
            if (ContextData == null) return;
            
            ContextData.Count = count;

            if (_notificationBadgeIcon)
            {
                _notificationBadgeIcon.SetActive(!ContextData.IsOwner && ContextData.Count > 0);
            }
        }

        protected override void OnInitialized()
        {
            _button.onClick.AddListener(ContextData.OnClick);
            
            _count.Initialize(ContextData);
            
            RefreshCount(ContextData.Count);
        }

        protected override void BeforeCleanUp()
        {
            _button.onClick.RemoveAllListeners();
            
            _count.CleanUp();
        }
    }
}