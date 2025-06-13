using Abstract;
using UnityEngine;

namespace UIManaging.EnhancedScrollerComponents
{
    public class VideoListItemRowStretched<V, M> : EnhancedScrollerItemsRow<V, M> where V : BaseContextDataView<M>
    {
        [SerializeField] private GameObject[] _spacers;
        
        protected override void AfterSetup(M[] itemsModels)
        {
            for (var i = 0; i < _spacers.Length; ++i)
            {
                _spacers[i].gameObject.SetActive(i + 1 >= itemsModels.Length);
                _spacers[i].transform.SetAsLastSibling();
            }

            base.AfterSetup(itemsModels);
        }
    }
}