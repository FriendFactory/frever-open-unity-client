using Abstract;
using Components;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UIManaging.EnhancedScrollerComponents
{
    public abstract class EnhancedScrollerItemsRow<V, M> : MonoBehaviour, IEnhancedScrollerRowItem<M> where V : BaseContextDataView<M>
    {
        [SerializeField] private ViewSpawner viewSpawner;
        protected V[] Views;

        public void Setup(M[] itemsModels)
        {
            itemsModels = BeforeSetup(itemsModels);
            Views = SpawnItems(itemsModels).ToArray();
            AfterSetup(itemsModels);
        }

        private IEnumerable<V> SpawnItems(M[] itemsModels) => viewSpawner.Spawn<M, V>(itemsModels);

        protected virtual M[] BeforeSetup(M[] itemsModels) => itemsModels;

        protected virtual void AfterSetup(M[] itemsModels) { }
    }
}
