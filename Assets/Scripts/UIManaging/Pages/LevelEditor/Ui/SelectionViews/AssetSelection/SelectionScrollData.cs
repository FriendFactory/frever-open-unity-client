using System.Collections.Generic;
using Extensions;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection
{
    public class SelectionScrollData
    {
        private readonly Dictionary<KeyValuePair<DbModelType, int>, long> _shrinkedCategoryPositions = new Dictionary<KeyValuePair<DbModelType, int>, long>();
        private readonly Dictionary<KeyValuePair<DbModelType, int>, long> _expandedCategoryPositions = new Dictionary<KeyValuePair<DbModelType, int>, long>();
        
        public long GetShrinkedPositionForCategory(DbModelType type, int categoryIndex)
        {
            return _shrinkedCategoryPositions.TryGetValue(new KeyValuePair<DbModelType, int>(type, categoryIndex), out var itemId)
                ? itemId
                : 0;
        }

        public long GetExpandedPositionForCategory(DbModelType type, int categoryIndex)
        {
            return _expandedCategoryPositions.TryGetValue(new KeyValuePair<DbModelType, int>(type, categoryIndex), out var itemId)
                ? itemId
                : 0;
        }
        
        public void SetShrinkedViewScrollPosition(long shrinkedViewScrollItemId, DbModelType type, int categoryIndex)
        {
            _shrinkedCategoryPositions[new KeyValuePair<DbModelType, int>(type, categoryIndex)] = shrinkedViewScrollItemId;
        }

        public void SetExpandedViewScrollPosition(long expandedViewScrollItemId, DbModelType type, int categoryIndex)
        {
            _expandedCategoryPositions[new KeyValuePair<DbModelType, int>(type, categoryIndex)] = expandedViewScrollItemId;
        }
    }
}