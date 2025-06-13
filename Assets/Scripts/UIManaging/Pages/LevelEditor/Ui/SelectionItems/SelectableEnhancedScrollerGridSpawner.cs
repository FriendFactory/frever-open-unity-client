using Modules.Amplitude;
using JetBrains.Annotations;
using UIManaging.EnhancedScrollerComponents.CellSpawners;
using UIManaging.Pages.LevelEditor.Ui.SelectionViews.AssetSelection;
using UnityEngine;
using Zenject;

namespace UIManaging.Pages.LevelEditor.Ui.SelectionItems
{
    public class SelectableEnhancedScrollerGridSpawner : EnhancedScrollerGridSpawner<AssetSelectionItemView, SelectableOptimizedItemsRow, AssetSelectionItemModel>
    {
        [Header("Shopping cart experiment")]
        [SerializeField] private bool _useAlternateRowSize;
        [SerializeField] private float _alternateRowSize = 340f;
        
        private AmplitudeManager _amplitudeManager;
        
        //---------------------------------------------------------------------
        // Ctors
        //---------------------------------------------------------------------
        
        [Inject]
        [UsedImplicitly]
        public void Construct(AmplitudeManager amplitudeManager)
        {
            _amplitudeManager = amplitudeManager;
        }
        
        //---------------------------------------------------------------------
        // Messages
        //---------------------------------------------------------------------
        
        protected override void Awake()
        {
            base.Awake();

            if (_useAlternateRowSize && _amplitudeManager.IsShoppingCartFeatureEnabled())
            {
                SetRowSize(_alternateRowSize);
            }
        }

        //---------------------------------------------------------------------
        // Public
        //---------------------------------------------------------------------

        public void SetRowSize(float rowSize)
        {
            RowSize = rowSize;
        }
    }
}