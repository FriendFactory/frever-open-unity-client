using System.Collections;
using Bridge.NotificationServer;
using EnhancedUI.EnhancedScroller;
using Modules.Notifications.NotificationItemModels;
using UnityEngine;
using UnityEngine.UI;

namespace UIManaging.Pages.NotificationPage.NotificationSelection.NotificationCellViews
{
    public abstract class NotificationCellView : EnhancedScrollerCellView
    {
        private const int MIN_LAYOUT_HEIGHT = 150;

        [SerializeField] ContentSizeFitter _textContentSizeFitter;
        [SerializeField] RectTransform _textRectTransform;
        
        private LayoutElement _layoutElement;

        protected virtual void Awake()
        {
            _layoutElement = GetComponent<LayoutElement>();
        }

        private void OnDisable()
        {
            _textContentSizeFitter.enabled = false;
        }

        public abstract NotificationType Type { get; }

        public abstract void Initialize(NotificationItemModel model);

        protected void RefreshLayout()
        {
            _textContentSizeFitter.enabled = true;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_textRectTransform);

            if (gameObject.activeSelf == false || gameObject.activeInHierarchy == false) return;
            StartCoroutine(RefreshMinLayoutElementHeight());
        }

        private IEnumerator RefreshMinLayoutElementHeight()
        {
            yield return new WaitForEndOfFrame();
            
            if (_textRectTransform.rect.height > MIN_LAYOUT_HEIGHT)
            {
                _layoutElement.minHeight = _textRectTransform.rect.height;
            }
        }
    }
}