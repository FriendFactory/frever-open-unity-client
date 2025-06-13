using Extensions;
using UnityEngine;

namespace UIManaging.Pages.DiscoveryPage
{
    internal abstract class ItemsDiscoverySection<T> : BaseDiscoverySection where T : MonoBehaviour
    {
        [Header("Items")]
        [SerializeField] private Transform _itemsParent;
        [SerializeField] protected T _itemPrefab;
        [SerializeField] protected int _itemsCount = 3;

        protected T[] Items;

        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------

        protected override void Awake()
        {
            base.Awake();
            Items = new T[_itemsCount];
            InstantiateItems();
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------

        private void InstantiateItems()
        {
            for (var i = 0; i < _itemsCount; i++)
            {
                Items[i] = Instantiate(_itemPrefab, _itemsParent);
                Items[i].SetActive(false);
            }
        }
    }
}