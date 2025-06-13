using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;

namespace UIManaging.EnhancedScrollerComponents.CellSpawners
{
    public class EnhancedScrollerSingleCellSpawner : BaseEnhancedScrollerSpawner
    {
        public override void InitializeCellView<V, M>(string prefabName, EnhancedScrollerCellView cellView, EnhancedScroller scroller, IEnumerable<M> dataCollection,
            int dataIndex, int cellIndex)
        {
            var view = cellView.GetComponent<V>();
            
#if UNITY_EDITOR
            view.name = $"[{prefabName}] {dataIndex}";
#endif
            view.Initialize(dataCollection.ElementAt(dataIndex));
        }
    }
}