using System.Linq;
using Abstract;
using EnhancedUI.EnhancedScroller;
using Modules.Notifications.NotificationItemModels;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews;
using UIManaging.Pages.NotificationPage.NotificationSelection.NotificationItemModels;
using UnityEngine;

namespace UIManaging.Pages.NotificationPage.NotificationSelection
{
    internal sealed class NotificationListView : BaseContextDataView<NotificationListModel>, IEnhancedScrollerDelegate
    {
        [SerializeField] private EnhancedScroller _enhancedScroller;
        [SerializeField] private NotificationCellView[] _notificationCellViews;
        [SerializeField] private EnhancedScrollerCellView _separatorView;
        [SerializeField] private EnhancedScrollerCellView _notificationsGroupCellView;

        private void Awake()
        {
            _enhancedScroller.Delegate = this;
        }

        protected override void OnInitialized()
        {
            _enhancedScroller.ReloadData();
        }

        public int GetNumberOfCells(EnhancedScroller scroller)
        {
            return ContextData?.ListItemModels?.Count ?? 0;
        }

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex)
        {
            var itemModel = ContextData.ListItemModels[dataIndex];

            return itemModel switch
            {
                NotificationsGroupModel _ => GetCellViewSize(_notificationsGroupCellView),
                NotificationItemModel _ => GetCellViewSize(_notificationCellViews.First()),
                NotificationTimePeriodSeparatorModel _ => GetCellViewSize(_separatorView),
                _ => 0
            };
        }

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var itemModel = ContextData.ListItemModels[dataIndex];

            EnhancedScrollerCellView cellView = null;
            
            switch (itemModel)
            {
                case NotificationsGroupModel model:
                {
                    cellView = scroller.GetCellView(_notificationsGroupCellView);
                    var viewComponent = cellView.GetComponent<NewLikesOnVideoCellView>();
                    viewComponent.Initialize(model);
                    viewComponent.Clicked -= OnGroupedNotificationClicked;
                    viewComponent.Clicked += OnGroupedNotificationClicked;
                    break;
                }
                case NotificationItemModel notificationItemModel:
                {
                    var cellPrefab = _notificationCellViews.First(x => x.Type == notificationItemModel.Type);
                    cellView = scroller.GetCellView(cellPrefab);
                    ((NotificationCellView)cellView).Initialize(notificationItemModel);
                    break;
                }
                case NotificationTimePeriodSeparatorModel separatorModel:
                {
                    cellView = scroller.GetCellView(_separatorView);
                    cellView.GetComponent<NotificationTimePeriodSeparatorView>().Text = separatorModel.Text;
                    break;
                }
            }

            return cellView;
        }
        
        public void Cleanup()
        {
            _enhancedScroller.ClearAll();
        }

        private static float GetCellViewSize(Component c)
        {
            return c.GetComponent<RectTransform>().rect.height;
        }

        private void OnGroupedNotificationClicked(NotificationsGroupModel groupModel)
        {
            if (!groupModel.Expanded)
            {
                var index = ContextData.ListItemModels.IndexOf(groupModel);
                ContextData.ListItemModels.InsertRange(index + 1, groupModel.GroupedModels);
            }
            else
            {
                foreach (var notificationModel in groupModel.GroupedModels)
                {
                    ContextData.ListItemModels.Remove(notificationModel);
                }
            }

            groupModel.Expanded = !groupModel.Expanded;
            var scrollPosition = _enhancedScroller.ScrollPosition;
            _enhancedScroller.ReloadDataAfterItem(ContextData.ListItemModels.IndexOf(groupModel));
            _enhancedScroller.SetScrollPositionImmediately(scrollPosition);
        }
    }
}