using System.Collections.Generic;
using Abstract;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.EnhancedScrollerComponents.CellSpawners
{
    public abstract class BaseEnhancedScrollerSpawner : MonoBehaviour
    {
        public abstract void InitializeCellView<V, M>(string prefabName, EnhancedScrollerCellView cellView,
            EnhancedScroller scroller, IEnumerable<M> dataCollection, int dataIndex, int cellIndex)
            where V : BaseContextDataView<M>;
    }
}