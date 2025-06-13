using UnityEngine;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionItems
{
    public sealed class RowData: MonoBehaviour
    {
        [SerializeField] private float _rowSize = 290;
        
        public float RowSize => _rowSize;
    }
}