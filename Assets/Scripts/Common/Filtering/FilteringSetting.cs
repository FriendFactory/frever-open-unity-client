using Bridge.Models.ClientServer.Assets;
using UnityEngine;

namespace Filtering
{
    public struct FilteringSetting
    {
        public AssetSorting Sorting;
        public AssetPriceFilter AssetPriceFilter;

        public bool IsNoFiltersSetted
        {
            get
            {
                return Sorting == AssetSorting.NewestFirst && AssetPriceFilter == AssetPriceFilter.None;
            }
        }
    }
}