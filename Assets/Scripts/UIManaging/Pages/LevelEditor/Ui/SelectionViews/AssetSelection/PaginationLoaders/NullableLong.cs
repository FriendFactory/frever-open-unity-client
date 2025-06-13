namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection.PaginationLoaders
{
    public class NullableLong
    {
        public bool HasValue;
        public long Value;

        public static implicit operator NullableLong(long? nullable) => new NullableLong
            { HasValue = nullable.HasValue, Value = nullable ?? 0 };

        public static implicit operator long?(NullableLong nullable) =>
            nullable?.HasValue ?? false ? (long?) nullable.Value : null;
    }
}