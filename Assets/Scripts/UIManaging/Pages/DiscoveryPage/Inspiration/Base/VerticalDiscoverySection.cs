using UnityEngine;

namespace UIManaging.Pages.DiscoveryPage
{
    internal abstract class VerticalDiscoverySection<T> : ItemsDiscoverySection<T> where T : MonoBehaviour
    {
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            UpdateHeight();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void UpdateHeight()
        {
            var headerHeight = _header.rect.height;
            var itemHeight = _itemPrefab.GetComponent<RectTransform>().rect.height;
            var totalHeight = headerHeight + itemHeight * _itemsCount;

            var rectTransform = GetComponent<RectTransform>();
            rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, totalHeight);
        }
    }
}