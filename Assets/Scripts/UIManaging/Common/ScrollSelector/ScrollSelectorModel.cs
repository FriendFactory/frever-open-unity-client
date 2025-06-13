namespace UIManaging.Common.ScrollSelector
{
    public interface IScrollSelectorModel
    {
        ScrollSelectorItemModel[] Items { get; }
        int InitialDataIndex { get; }
    }

    public class ScrollSelectorModel
    {
        public ScrollSelectorItemModel[] Items { get; private set; }
        public int InitialDataIndex { get; private set; }

        public ScrollSelectorModel(ScrollSelectorItemModel[] items, int initialDataIndex)
        {
            Items = items;
            InitialDataIndex = initialDataIndex;
        }
    }
}


