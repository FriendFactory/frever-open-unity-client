namespace UIManaging.Pages.Feed.Remix.Collection
{
    internal class CharacterCellViewModel
    {
        public int Index { get; }
        public bool IsHeader { get; }
        
        public CharacterCellViewModel(int index, bool isHeader = false)
        {
            Index = index;
            IsHeader = isHeader;
        }
    }
}