using System.Collections.Generic;
using System.Linq;
using Abstract;
using UnityEngine;

namespace UIManaging.PopupSystem.Popups.Shuffle
{
    public class AssetTypeListView : BaseContextDataView<AssetTypeListModel>
    {
        [SerializeField] private RectTransform _assetTypeViewContainer;
        [SerializeField] private AssetTypeView _assetTypeViewPrefab;
        [SerializeField] private List<AssetTypeView> _assetTypeViews = new List<AssetTypeView>();
        
        //---------------------------------------------------------------------
        // Protected
        //---------------------------------------------------------------------

        protected override void OnInitialized()
        {
            AdjustElementAmount();

            for (var i = 0; i < ContextData.Items.Count; i++)
            {
                _assetTypeViews[i].Initialize(ContextData.Items[i]);
            }
        }

        protected override void BeforeCleanup()
        {
            base.BeforeCleanup();

            foreach (var assetTypeView in _assetTypeViews)
            {
                assetTypeView.CleanUp();
            }
        }

        //---------------------------------------------------------------------
        // Helpers
        //---------------------------------------------------------------------
        
        private void AdjustElementAmount()
        {
            if (_assetTypeViews.Count > ContextData.Items.Count)
            {
                foreach (var assetTypeView in _assetTypeViews.Skip(ContextData.Items.Count))
                {
                    Destroy(assetTypeView.gameObject);
                }
                
                _assetTypeViews.RemoveRange(ContextData.Items.Count, _assetTypeViews.Count - ContextData.Items.Count);
            }

            if (_assetTypeViews.Count < ContextData.Items.Count)
            {
                for (var i = _assetTypeViews.Count; i < ContextData.Items.Count; i++)
                {
                    var assetTypeView = Instantiate(_assetTypeViewPrefab, _assetTypeViewContainer);
                    
                    _assetTypeViews.Add(assetTypeView);
                }
            }
        }
    }
}