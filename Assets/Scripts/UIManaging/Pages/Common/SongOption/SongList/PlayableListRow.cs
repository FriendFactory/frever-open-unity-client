using Abstract;
using Common.Abstract;
using EnhancedUI.EnhancedScroller;
using UIManaging.EnhancedScrollerComponents;

namespace UIManaging.Pages.Common.SongOption.SongList
{
    public abstract class PlayableListRow<TItem, TModel> : EnhancedScrollerOptimizedItemsRow<TItem, TModel>, IContextInitializable<TModel[]>
        where TItem : BaseContextDataView<TModel>
        where TModel : PlayableItemModel
    {
        public TModel[] ContextData { get; private set; }
        public bool IsInitialized { get; private set; }
        
        public override EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(CellViewPrefab);
            var view = cellView.GetComponent<TItem>();

#if UNITY_EDITOR
            view.name = $"[{CellViewPrefab.name}] {dataIndex}";
#endif
            // initialize PlaybleItem once in order to prevent clean up and following thumbnail re-downloading 
            // already initialized view may be reused with different model, that's why id check is added 
            var model = Items[dataIndex];
            if (!view.IsInitialized || model.Id != view.ContextData?.Id)
            {
                view.Initialize(Items[dataIndex]);
            }
            return cellView;
        }

        public void Initialize(TModel[] model)
        {
            ContextData = model;
            
            Setup(ContextData);

            IsInitialized = true;
        }

        public void CleanUp()
        {
            EnhancedScroller.ClearAll();

            ContextData = default;
            IsInitialized = false;
        }
    }
}