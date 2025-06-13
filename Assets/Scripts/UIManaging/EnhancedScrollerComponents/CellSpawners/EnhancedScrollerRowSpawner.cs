using System.Collections.Generic;
using System.Linq;
using EnhancedUI.EnhancedScroller;
using UnityEngine;

namespace UIManaging.EnhancedScrollerComponents.CellSpawners
{
    public class EnhancedScrollerRowSpawner : BaseEnhancedScrollerSpawner
    {
        [SerializeField] private int _itemsInRow = 3;

        public void SetItemsInRow(int itemsInRow)
        {
            _itemsInRow = itemsInRow;
        }
        
        public override void InitializeCellView<V, M>(string prefabName, EnhancedScrollerCellView cellView, EnhancedScroller scroller, IEnumerable<M> dataCollection, int dataIndex, int cellIndex)
        {
            var itemsToSkip = dataIndex * _itemsInRow;
            var models = dataCollection.Skip(itemsToSkip).Take(_itemsInRow).ToArray();
            var view = cellView.GetComponent<IEnhancedScrollerRowItem<M>>();
            
#if UNITY_EDITOR
            cellView.name = $"[{prefabName}] {itemsToSkip}-{itemsToSkip + models.Length - 1}";
#endif
            
            view.Setup(models);
        }
    }
}